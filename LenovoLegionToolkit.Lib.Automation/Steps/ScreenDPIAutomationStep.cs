using System;
using System.Threading.Tasks;
using CCD.Enum;
using CCD;
using LenovoLegionToolkit.Lib.Utils;
using Newtonsoft.Json;

namespace LenovoLegionToolkit.Lib.Automation.Steps
{
    public class ScreenDPIAutomationStep : IAutomationStep<ScreenDPI>
    {
        public ScreenDPI State { get; }

        [JsonConstructor]
        public ScreenDPIAutomationStep(ScreenDPI dpi) => State = dpi;

        public Task<bool> IsSupportedAsync() => Task.FromResult(true);

        public Task<ScreenDPI[]> GetAllStatesAsync() => Task.FromResult(new ScreenDPI[] {
            new(100),
            new(125),
            new(150),
            new(175),
            new(200),
        });

        public IAutomationStep DeepCopy() => new ScreenDPIAutomationStep(State);

        public Task RunAsync() => ResetHWMonitorDPI();


        private async Task ResetHWMonitorDPI()
        {
            DisplayConfigTopologyId topologyId;
            var list = CCDHelpers.GetPathWraps(QueryDisplayFlags.OnlyActivePaths, out topologyId);
            foreach (var item in list)
            {
                var sourceModeInfo = item.Path.sourceInfo;
                var targetModeInfo = item.Path.targetInfo;
                var name = CCDHelpers.GetTargetDeviceName(targetModeInfo);

                if (!name.StartsWith("HW", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var dpiInfo = CCDHelpers.GetDPIScalingInfo(sourceModeInfo.adapterId, sourceModeInfo.id);

                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"hw screen name: {name}, current dpi: {dpiInfo.current}");

                if (dpiInfo.current != dpiInfo.recommended)
                {
                    var result = CCDHelpers.SetDPIScaling(sourceModeInfo.adapterId, sourceModeInfo.id, dpiInfo.recommended);
                    Log.Instance.Trace($"set recommended dpi: {dpiInfo.recommended}");
                    MessagingCenter.Publish(new Notification(NotificationType.ScreenDPISet, NotificationDuration.Long, name));
                }
            }
        }

    }
}
