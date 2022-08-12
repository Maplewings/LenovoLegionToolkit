﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Extensions;
using LenovoLegionToolkit.Lib.Features;
using LenovoLegionToolkit.Lib.System;
using LenovoLegionToolkit.Lib.Utils;
using LenovoLegionToolkit.WPF.Utils;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace LenovoLegionToolkit.WPF.Controls.Dashboard
{
    public class HybridModeControl : ContentControl
    {
        public HybridModeControl()
        {
            Initialized += HybridModeControl_Initialized;
        }

        private async void HybridModeControl_Initialized(object? sender, EventArgs e)
        {
            var mi = await Compatibility.GetMachineInformation();
            if (mi.Properties.SupportsExtendedHybridMode)
                Content = new ComboBoxHybridModeControl();
            else
                Content = new ToggleHybridModeControl();
        }
    }

    public class ComboBoxHybridModeControl : AbstractComboBoxFeatureCardControl<HybridModeState>
    {
        private bool _ignoreNextStateChange;

        public ComboBoxHybridModeControl()
        {
            Icon = SymbolRegular.LeafOne24;
            Title = "GPU Working Mode";
            Subtitle = "Select GPU operating mode based on your computer's usage and power conditions.\nSwitching modes may require restart.";
        }

        protected override UIElement? GetAccessory()
        {
            var element = base.GetAccessory();
            _comboBox.Width = 180;
            return element;
        }

        protected override async Task OnStateChange(ComboBox comboBox, IFeature<HybridModeState> feature, HybridModeState? newValue, HybridModeState? oldValue)
        {
            if (_ignoreNextStateChange)
            {
                _ignoreNextStateChange = false;
                return;
            }

            if (newValue is null || oldValue is null)
                return;

            if (newValue == HybridModeState.Off || oldValue == HybridModeState.Off)
            {
                var result = await MessageBoxHelper.ShowAsync(
                    this,
                    "Restart required",
                    $"Changing to {newValue.GetDisplayName()} requires restart. Do you want to restart now?");

                if (result)
                {
                    await base.OnStateChange(comboBox, feature, newValue, oldValue);
                    await Power.RestartAsync();
                }
                else
                {
                    _ignoreNextStateChange = true;
                    comboBox.SelectItem(oldValue.Value);
                }

                return;
            }

            await base.OnStateChange(comboBox, feature, newValue, oldValue);
        }
    }

    public class ToggleHybridModeControl : AbstractToggleFeatureCardControl<HybridModeState>
    {
        protected override HybridModeState OnState => HybridModeState.On;

        protected override HybridModeState OffState => HybridModeState.Off;

        public ToggleHybridModeControl()
        {
            Icon = SymbolRegular.LeafOne24;
            Title = "Hybrid Mode";
            Subtitle = "Allow switching between integrated and discrete GPU.\nRequires restart.";
        }

        protected override async Task OnStateChange(ToggleSwitch toggle, IFeature<HybridModeState> feature)
        {
            var result = await MessageBoxHelper.ShowAsync(
                this,
                "Restart required",
                "Changing Hybrid Mode requires restart. Do you want to restart now?");

            if (result)
            {
                await base.OnStateChange(toggle, feature);
                await Power.RestartAsync();
            }
            else
                toggle.IsChecked = !toggle.IsChecked;
        }
    }
}
