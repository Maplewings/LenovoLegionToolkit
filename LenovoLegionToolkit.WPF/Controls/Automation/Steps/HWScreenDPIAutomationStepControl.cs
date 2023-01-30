using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Automation;
using LenovoLegionToolkit.Lib.Automation.Steps;
using LenovoLegionToolkit.WPF.Resources;
using Wpf.Ui.Common;

namespace LenovoLegionToolkit.WPF.Controls.Automation.Steps
{
    public class HWScreenDPIAutomationStepControl : AbstractComboBoxAutomationStepCardControl<DpiScale>
    {
        public HWScreenDPIAutomationStepControl(IAutomationStep<DpiScale> dpi) : base(dpi)
        {
            Icon = SymbolRegular.Laptop24;
            Title = Resource.HWScreenDPIAutomationStepControl_Title;
            Subtitle = Resource.HWScreenDPIAutomationStepControl_Message;
        }
    }
}
