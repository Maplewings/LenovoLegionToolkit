using System;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Extensions;
using LenovoLegionToolkit.Lib.Utils;
using Newtonsoft.Json;
using WindowsDisplayAPI;
using WindowsDisplayAPI.Native.DisplayConfig;

namespace LenovoLegionToolkit.Lib.Automation.Steps;

public class HWScreenDPIAutomationStep : IAutomationStep<DpiScale>
{
    public DpiScale State { get; }

    [JsonConstructor]
    public HWScreenDPIAutomationStep(DpiScale state) => State = state;

    public Task<bool> IsSupportedAsync() => Task.FromResult(true);

    public Task<DpiScale[]> GetAllStatesAsync() => Task.FromResult(new DpiScale[] {
        new(100),
        new(125),
        new(150),
        new(175),
        new(200),
    });

    public IAutomationStep DeepCopy() => new HWScreenDPIAutomationStep(State);

    public Task RunAsync() => ResetHWMonitorDPI();

    private Task ResetHWMonitorDPI()
    {
        foreach (var item in Display.GetDisplays())
        {
            var name = item.GetTargetDeviceName();
            if (!name.StartsWith("HW", StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }
            var dpiInfo = item.GetDisplaScaleInfo();

            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"hw screen name: {name}, current dpi: {dpiInfo.current}");

            if (dpiInfo.current != State.Scale)
            {
                item.ToPathDisplaySource().CurrentDPIScale = (DisplayConfigSourceDPIScale)(uint)State.Scale;
                Log.Instance.Trace($"set screen dpi: {State.DisplayName}");
                MessagingCenter.Publish(new Notification(NotificationType.ScreenDPISet, NotificationDuration.Long, name));
            }

        }

        return Task.CompletedTask;
    }
}

