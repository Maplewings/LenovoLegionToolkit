﻿using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.Features;

namespace LenovoLegionToolkit.Lib.Automation.Steps
{
    public class OverDriveAutomationStep : IAutomationStep<OverDriveState>
    {
        private readonly OverDriveFeature _feature = DIContainer.Resolve<OverDriveFeature>();

        public OverDriveState State { get; }

        public OverDriveAutomationStep(OverDriveState state)
        {
            State = state;
        }

        public async Task RunAsync()
        {
            var currentState = await _feature.GetStateAsync().ConfigureAwait(false);
            if (State == currentState)
                return;
            await _feature.SetStateAsync(State).ConfigureAwait(false);
        }

        public Task<OverDriveState[]> GetAllStatesAsync() => _feature.GetAllStatesAsync();

        public IAutomationStep DeepCopy() => new OverDriveAutomationStep(State);
    }
}
