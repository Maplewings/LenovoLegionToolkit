﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Utils;

namespace LenovoLegionToolkit.Lib.System
{
    public class Vantage : SoftwareDisabler
    {
        protected override string[] ScheduledTasksPaths => new[]
        {
            "Lenovo\\BatteryGauge",
            "Lenovo\\ImController",
            "Lenovo\\ImController\\Plugins",
            "Lenovo\\ImController\\TimeBasedEvents",
            "Lenovo\\Vantage",
            "Lenovo\\Vantage\\Schedule",
        };

        protected override string[] ServiceNames => new[]
        {
            "ImControllerService",
            "LenovoVantageService",
        };

        public override async Task DisableAsync()
        {
            await base.DisableAsync().ConfigureAwait(false);

            foreach (var process in Process.GetProcessesByName("LenovoVantageService"))
            {
                try
                {
                    process.Kill();
                    await process.WaitForExitAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (Log.Instance.IsTraceEnabled)
                        Log.Instance.Trace($"Couldn't kill process: {ex.Demystify()}");
                }
            }

            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName.StartsWith("Lenovo.Modern.ImController", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        process.Kill();
                        await process.WaitForExitAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (Log.Instance.IsTraceEnabled)
                            Log.Instance.Trace($"Couldn't kill process: {ex.Demystify()}");
                    }
                }

                if (process.ProcessName.StartsWith("LenovoVantage", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        process.Kill();
                        await process.WaitForExitAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (Log.Instance.IsTraceEnabled)
                            Log.Instance.Trace($"Couldn't kill process: {ex.Demystify()}");
                    }
                }
            }
        }
    }
}
