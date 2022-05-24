﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Utils;
using LenovoLegionToolkit.WPF.Extensions;
using LenovoLegionToolkit.WPF.Pages;
using WPFUI.Controls;
using WPFUI.Controls.Interfaces;
using Container = LenovoLegionToolkit.WPF.Utils.Container;

namespace LenovoLegionToolkit.WPF.Windows
{
    public partial class MainWindow
    {
        private readonly UpdateChecker updateChecker = Container.Resolve<UpdateChecker>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeNavigation();
            InitializeTray();
            RestoreWindowSize();

            ResizeMode = ResizeMode.CanMinimize;

            _titleBar.UseSnapLayout = false;
            _titleBar.CanMaximize = false;
            _titleBar.ShowMaximize = false;

#if DEBUG
            _title.Text += " [DEBUG]";
#endif

            if (Log.Instance.IsTraceEnabled)
                _title.Text += " [TRACE ENABLED]";
        }

        private void InitializeNavigation()
        {
            RootNavigation.Frame = RootFrame;
            RootNavigation.Items = new ObservableCollection<INavigationItem>
            {
                new NavigationItem() { Icon = WPFUI.Common.SymbolRegular.Home20, Content = "Dashboard", PageTag = "dashboard", Page = typeof(DashboardPage)},
            };
            RootNavigation.Footer = new ObservableCollection<INavigationItem>
            {
                new NavigationItem() { Icon = WPFUI.Common.SymbolRegular.Settings28, Content = "Settings", PageTag = "settings", Page = typeof(SettingsPage)},
                new NavigationItem() { Icon = WPFUI.Common.SymbolRegular.Info28, Content = "About", PageTag = "about", Page = typeof(AboutPage)},
            };

            RootNavigation.Navigate(RootNavigation.Items[0].PageTag);
        }

        private void InitializeTray()
        {
            var openMenuItem = new MenuItem { Header = "Open" };
            openMenuItem.Click += (s, e) => BringToForeground();

            var closeMenuItem = new MenuItem { Header = "Close" };
            closeMenuItem.Click += (s, e) => Application.Current.Shutdown();

            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(openMenuItem);
            contextMenu.Items.Add(closeMenuItem);

            var notifyIcon = new NotifyIcon
            {
                TooltipText = "Lenovo Legion Toolkit",
                Icon = ImageSourceExtensions.FromResource("icon.ico"),
                FocusOnLeftClick = false,
                MenuOnRightClick = true,
                Menu = contextMenu,
            };
            notifyIcon.LeftClick += NotifyIcon_LeftClick;

            _titleBar.Tray = notifyIcon;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (Log.Instance.IsTraceEnabled)
                Log.Instance.Trace($"Window state changed to {WindowState}");

            switch (WindowState)
            {
                case WindowState.Minimized:
                    SendToTray();
                    break;
                case WindowState.Normal:
                    BringToForeground();
                    break;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveWindowSize();

            if (Settings.Instance.MinimizeOnClose)
            {
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"Minimizing...");

                WindowState = WindowState.Minimized;
                e.Cancel = true;
            }
            else
            {
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"Closing...");

                _titleBar.Tray.Unregister();
            }
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
                return;

            Task.Run(async () =>
            {
                await Task.Delay(2000);
                return await updateChecker.Check();
            }).ContinueWith(updatesAvailable =>
            {
                _updateIndicator.Visibility = updatesAvailable.Result ? Visibility.Visible : Visibility.Collapsed;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateIndicator_Click(object sender, RoutedEventArgs e)
        {
            var updateWindow = new UpdateWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            updateWindow.ShowDialog();
        }

        private void NotifyIcon_LeftClick([NotNull] INotifyIcon sender, RoutedEventArgs e) => BringToForeground();

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void SaveWindowSize()
        {
            Settings.Instance.WindowSize = new(ActualWidth, ActualHeight);
            Settings.Instance.Synchronize();
        }

        private void RestoreWindowSize()
        {
            var windowSize = Settings.Instance.WindowSize;
            if (windowSize.Width >= MinWidth && windowSize.Height >= MinHeight)
            {
                Width = windowSize.Width;
                Height = windowSize.Height;
            }
        }

        public void BringToForeground()
        {
            ShowInTaskbar = true;

            if (WindowState == WindowState.Minimized || Visibility == Visibility.Hidden)
            {
                Show();
                WindowState = WindowState.Normal;
            }

            Activate();
            Topmost = true;
            Topmost = false;
            Focus();

            _titleBar.Tray.Unregister();
        }

        public void SendToTray()
        {
            _titleBar.Tray.Register();

            Hide();
            ShowInTaskbar = false;
        }
    }
}
