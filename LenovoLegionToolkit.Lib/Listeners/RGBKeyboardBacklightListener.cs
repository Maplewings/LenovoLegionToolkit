﻿using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Controllers;
using LenovoLegionToolkit.Lib.Utils;

namespace LenovoLegionToolkit.Lib.Listeners
{
    public class RGBKeyboardBacklightListener : AbstractWMIListener<RGBKeyboardBacklightChanged>
    {
        private readonly RGBKeyboardBacklightController _controller;

        public RGBKeyboardBacklightListener(RGBKeyboardBacklightController controller) : base("ROOT\\WMI", "LENOVO_GAMEZONE_LIGHT_PROFILE_CHANGE_EVENT")
        {
            _controller = controller;
        }

        protected override RGBKeyboardBacklightChanged GetValue(PropertyDataCollection properties) => default;

        protected async override Task OnChangedAsync(RGBKeyboardBacklightChanged value)
        {
            try
            {
                // TODO: Find a way to disable Fn+Space on hardware level and get rid of this delay to reduce flickering
                await Task.Delay(500);
                await _controller.SetNextPresetAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"Failed to set next keyboard backlight preset: {ex.Demystify()}");
            }
        }
    }
}
