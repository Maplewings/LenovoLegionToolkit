using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.System;
using LenovoLegionToolkit.Lib.Utils;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using WindowsDisplayAPI;

namespace LenovoLegionToolkit.Lib.Extensions
{
    public static class DisplayExtensions
    {
        static uint[] DpiVals = { 100, 125, 150, 175, 200, 225, 250, 300, 350, 400, 450, 500 };

        public static async Task<Display?> GetBuiltInDisplayAsync()
        {
            var displays = Display.GetDisplays();

            if (Log.Instance.IsTraceEnabled)
            {
                Log.Instance.Trace($"Found displays:");
                foreach (var display in displays)
                    Log.Instance.Trace($" - {display}");
            }

            foreach (var display in Display.GetDisplays())
                if (await display.IsInternalAsync().ConfigureAwait(false))
                    return display;

            return null;
        }

        public static DisplayAdvancedColorInfo GetAdvancedColorInfo(this Display display)
        {
            var getAdvancedColorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
            getAdvancedColorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
            getAdvancedColorInfo.header.size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO));
            getAdvancedColorInfo.header.adapterId.HighPart = display.Adapter.ToPathDisplayAdapter().AdapterId.HighPart;
            getAdvancedColorInfo.header.adapterId.LowPart = display.Adapter.ToPathDisplayAdapter().AdapterId.LowPart;
            getAdvancedColorInfo.header.id = display.ToPathDisplayTarget().TargetId;

            if (PInvoke.DisplayConfigGetDeviceInfo(ref getAdvancedColorInfo.header) != 0)
                PInvokeExtensions.ThrowIfWin32Error("GetAdvancedColorInfo");

            return new(getAdvancedColorInfo.Anonymous.value.GetNthBit(0),
                getAdvancedColorInfo.Anonymous.value.GetNthBit(1),
                getAdvancedColorInfo.Anonymous.value.GetNthBit(2),
                getAdvancedColorInfo.Anonymous.value.GetNthBit(3));
        }

        public static void SetAdvancedColorState(this Display display, bool state)
        {
            var setAdvancedColorState = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE();
            setAdvancedColorState.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
            setAdvancedColorState.header.size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE));
            setAdvancedColorState.header.adapterId.HighPart = display.Adapter.ToPathDisplayAdapter().AdapterId.HighPart;
            setAdvancedColorState.header.adapterId.LowPart = display.Adapter.ToPathDisplayAdapter().AdapterId.LowPart;
            setAdvancedColorState.header.id = display.ToPathDisplayTarget().TargetId;

            setAdvancedColorState.Anonymous.value = setAdvancedColorState.Anonymous.value.SetNthBit(0, state);

            if (PInvoke.DisplayConfigSetDeviceInfo(setAdvancedColorState.header) != 0)
                PInvokeExtensions.ThrowIfWin32Error("SetAdvancedColorState");
        }

        public static string GetTargetDeviceName(this Display display) 
        {
            return display.DevicePath
                .Split("#")
                .Skip(1)
                .Take(2)
                .Aggregate((s1, s2) => s1 + "\\" + s2);

            var deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
            deviceName.header.type = (DISPLAYCONFIG_DEVICE_INFO_TYPE)(-3);
            deviceName.header.size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME));
            deviceName.header.adapterId.HighPart = display.Adapter.ToPathDisplayAdapter().AdapterId.HighPart;
            deviceName.header.adapterId.LowPart = display.Adapter.ToPathDisplayAdapter().AdapterId.LowPart;
            deviceName.header.id = display.ToPathDisplayTarget().TargetId;

            if (PInvoke.DisplayConfigGetDeviceInfo(ref deviceName.header) != 0)
                PInvokeExtensions.ThrowIfWin32Error("GetTargetDeviceName");
            return "";
        }

        public static unsafe DisplaScaleInfo GetDisplaScaleInfo(this Display display) 
        {
            var dpiInfo = new DisplaScaleInfo();

            var requestPacket = new DisplayConfigSourceDPIScaleGet();
            requestPacket.header.type = (DISPLAYCONFIG_DEVICE_INFO_TYPE)(-4);
            requestPacket.header.size = (uint)Marshal.SizeOf(typeof(DisplayConfigSourceDPIScaleGet));
            requestPacket.header.adapterId.HighPart = display.Adapter.ToPathDisplayAdapter().AdapterId.HighPart;
            requestPacket.header.adapterId.LowPart = display.Adapter.ToPathDisplayAdapter().AdapterId.LowPart;
            requestPacket.header.id = display.ToPathDisplayTarget().TargetId;

            DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacketLocal = (DISPLAYCONFIG_DEVICE_INFO_HEADER*)&requestPacket.header;
            int __result = PInvoke.DisplayConfigGetDeviceInfo(requestPacketLocal);

            if (requestPacket.curScaleRel < requestPacket.minScaleRel)
            {
                requestPacket.curScaleRel = requestPacket.minScaleRel;
            }
            else if (requestPacket.curScaleRel > requestPacket.maxScaleRel)
            {
                requestPacket.curScaleRel = requestPacket.maxScaleRel;
            }

            int minAbs = Math.Abs(requestPacket.minScaleRel);
            if (DpiVals.Length >= (minAbs + requestPacket.maxScaleRel + 1))
            {
                dpiInfo.current = DpiVals[minAbs + requestPacket.curScaleRel];
                dpiInfo.recommended = DpiVals[minAbs];
                dpiInfo.maximum = DpiVals[minAbs + requestPacket.maxScaleRel];
            }
            return dpiInfo;
        }

        public static bool SetDisplaScaleInfo(this Display display, uint dpiPercentToSet)
        {
            var dPIScalingInfo = display.GetDisplaScaleInfo();

            if (dpiPercentToSet == dPIScalingInfo.current)
            {
                return true;
            }

            if (dpiPercentToSet < dPIScalingInfo.mininum)
            {
                dpiPercentToSet = dPIScalingInfo.mininum;
            }
            else if (dpiPercentToSet > dPIScalingInfo.maximum)
            {
                dpiPercentToSet = dPIScalingInfo.maximum;
            }

            int idx1 = -1, idx2 = -1;

            int i = 0;
            foreach (var val in DpiVals)
            {
                if (val == dpiPercentToSet)
                {
                    idx1 = i;
                }

                if (val == dPIScalingInfo.recommended)
                {
                    idx2 = i;
                }
                i++;
            }

            if ((idx1 == -1) || (idx2 == -1))
            {
                return false;
            }

            int dpiRelativeVal = idx1 - idx2;

            var setPacket = new DisplayConfigSourceDPIScaleSet();
            setPacket.header.type = (DISPLAYCONFIG_DEVICE_INFO_TYPE)(-4);
            setPacket.header.size = (uint)Marshal.SizeOf(typeof(DisplayConfigSourceDPIScaleSet));
            setPacket.header.adapterId.HighPart = display.Adapter.ToPathDisplayAdapter().AdapterId.HighPart;
            setPacket.header.adapterId.LowPart = display.Adapter.ToPathDisplayAdapter().AdapterId.LowPart;
            setPacket.header.id = display.ToPathDisplayTarget().TargetId;
            setPacket.scaleRel = dpiRelativeVal;
            var res = PInvoke.DisplayConfigSetDeviceInfo(in setPacket.header);
            return res != 0;
        }


        private static async Task<bool> IsInternalAsync(this Device display)
        {
            var instanceName = display.DevicePath
                .Split("#")
                .Skip(1)
                .Take(2)
                .Aggregate((s1, s2) => s1 + "\\" + s2);

            var result = await WMI.ReadAsync("root\\WMI",
                $"SELECT * FROM WmiMonitorConnectionParams WHERE InstanceName LIKE '%{instanceName}%'",
                pdc => (uint)pdc["VideoOutputTechnology"].Value).ConfigureAwait(false);
            var vot = result.FirstOrDefault();

            const uint votInternal = 0x80000000;
            const uint votDisplayPortEmbedded = 11;
            return vot is votInternal or votDisplayPortEmbedded;
        }
    }
}
