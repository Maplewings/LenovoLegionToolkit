﻿using System;
using Autofac;
using LenovoLegionToolkit.Lib.Controllers;
using LenovoLegionToolkit.Lib.Features;
using LenovoLegionToolkit.Lib.Listeners;
using LenovoLegionToolkit.Lib.Utils;

namespace LenovoLegionToolkit.WPF.Utils
{
    public class Container
    {
        private static IContainer? _container;

        public static void Initialize()
        {
            var cb = new ContainerBuilder();

            cb.Register<ThemeManager>();

            // Lib
            cb.Register<AlwaysOnUSBFeature>();
            cb.Register<BatteryFeature>();
            cb.Register<FlipToStartFeature>();
            cb.Register<FnLockFeature>();
            cb.Register<HybridModeFeature>();
            cb.Register<OverDriveFeature>();
            cb.Register<PowerModeFeature>();
            cb.Register<RefreshRateFeature>();
            cb.Register<TouchpadLockFeature>();
            cb.Register<PowerModeListener>();
            cb.Register<GPUController>();
            cb.Register<CPUBoostModeController>();
            cb.Register<UpdateChecker>();

            _container = cb.Build();
        }

        public static T Resolve<T>() where T : notnull
        {
            if (_container == null)
                throw new InvalidOperationException("Container must be initialized first");
            return _container.Resolve<T>();
        }
    }

    static class ContainerBuilderExtensions
    {
        public static void Register<T>(this ContainerBuilder cb) where T : notnull
        {
            cb.RegisterType<T>().AsSelf().AsImplementedInterfaces().SingleInstance();
        }
    }
}
