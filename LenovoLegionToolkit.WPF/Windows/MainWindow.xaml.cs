﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Utils;
using LenovoLegionToolkit.WPF.Extensions;
using LenovoLegionToolkit.WPF.Pages;
using WPFUI.Controls;
using WPFUI.Tray;

namespace LenovoLegionToolkit.WPF.Windows
{
    public partial class MainWindow : Window
    {
        private readonly Lazy<NotifyIcon> _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNavigation();

            _notifyIcon = new Lazy<NotifyIcon>(CreateNotifyIcon);
        }

        private void InitializeNavigation()
        {
            RootNavigation.Frame = RootFrame;
            RootNavigation.Items = new ObservableCollection<NavigationItem>
            {
                new() { Icon = WPFUI.Common.Icon.Home20, Name = "Dashboard", Tag = "dashboard", Type = typeof(Dashboard)},
            };
            RootNavigation.Footer = new ObservableCollection<NavigationItem>
            {
                new() { Icon = WPFUI.Common.Icon.Settings20, Name = "Settings", Tag = "settings", Type = typeof(SettingsPage)},
                new() { Icon = WPFUI.Common.Icon.Info20, Name = "About", Tag = "about", Type = typeof(AboutPage)},
            };

            RootNavigation.Navigate("dashboard");
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

                _notifyIcon.Value.Destroy();
            }
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
                return;

            Task.Run(Updates.Check)
                .ContinueWith(updatesAvailable =>
                {
                    _updateIndicator.Visibility = updatesAvailable.Result ? Visibility.Visible : Visibility.Collapsed;
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private NotifyIcon CreateNotifyIcon()
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
                Tooltip = "Lenovo Legion Toolkit",
                Icon = ImageSourceExtensions.FromResource("icon.ico"),
                Parent = this,
                ContextMenu = contextMenu,
                Click = _ => BringToForeground(),
            };
            return notifyIcon;
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

            _notifyIcon.Value.Destroy();
        }

        public void SendToTray()
        {
            _notifyIcon.Value.Show();

            Hide();
            ShowInTaskbar = false;
        }
    }
}
