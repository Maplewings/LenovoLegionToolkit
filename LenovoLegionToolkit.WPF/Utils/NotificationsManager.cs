﻿using System;
using System.Windows;
using System.Windows.Threading;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Settings;
using LenovoLegionToolkit.WPF.Resources;
using LenovoLegionToolkit.WPF.Windows.Utils;
using Wpf.Ui.Common;

namespace LenovoLegionToolkit.WPF.Utils
{
    public class NotificationsManager
    {
        private Dispatcher Dispatcher => Application.Current.Dispatcher;

        private readonly ApplicationSettings _settings;

        private NotificationWindow? _window;

        public NotificationsManager(ApplicationSettings settings)
        {
            _settings = settings;

            MessagingCenter.Subscribe<Notification>(this, OnNotificationReceived);
        }

        private void OnNotificationReceived(Notification notification) => Dispatcher.Invoke(() =>
        {
            if (_settings.Store.DontShowNotifications)
                return;

            if (FullscreenHelper.IsAnyApplicationFullscreen())
                return;

            var allow = notification.Type switch
            {
                NotificationType.CapsLockOn => _settings.Store.Notifications.CapsNumLock,
                NotificationType.CapsLockOff => _settings.Store.Notifications.CapsNumLock,
                NotificationType.CameraOn => _settings.Store.Notifications.CameraLock,
                NotificationType.CameraOff => _settings.Store.Notifications.CameraLock,
                NotificationType.FnLockOn => _settings.Store.Notifications.FnLock,
                NotificationType.FnLockOff => _settings.Store.Notifications.FnLock,
                NotificationType.MicrophoneOn => _settings.Store.Notifications.Microphone,
                NotificationType.MicrophoneOff => _settings.Store.Notifications.Microphone,
                NotificationType.NumLockOn => _settings.Store.Notifications.CapsNumLock,
                NotificationType.NumLockOff => _settings.Store.Notifications.CapsNumLock,
                NotificationType.PowerMode => _settings.Store.Notifications.PowerMode,
                NotificationType.RefreshRate => _settings.Store.Notifications.RefreshRate,
                NotificationType.TouchpadOn => _settings.Store.Notifications.TouchpadLock,
                NotificationType.TouchpadOff => _settings.Store.Notifications.TouchpadLock,
                NotificationType.WhiteKeyboardBacklight => _settings.Store.Notifications.KeyboardBacklight,
                _ => throw new ArgumentException(nameof(notification.Type))
            };

            if (!allow)
                return;

            var symbol = notification.Type switch
            {
                NotificationType.CapsLockOn => SymbolRegular.KeyboardShiftUppercase24,
                NotificationType.CapsLockOff => SymbolRegular.KeyboardShiftUppercase24,
                NotificationType.CameraOn => SymbolRegular.Camera24,
                NotificationType.CameraOff => SymbolRegular.Camera24,
                NotificationType.FnLockOn => SymbolRegular.Keyboard24,
                NotificationType.FnLockOff => SymbolRegular.Keyboard24,
                NotificationType.MicrophoneOn => SymbolRegular.Mic24,
                NotificationType.MicrophoneOff => SymbolRegular.Mic24,
                NotificationType.NumLockOn => SymbolRegular.Keyboard12324,
                NotificationType.NumLockOff => SymbolRegular.Keyboard12324,
                NotificationType.PowerMode => SymbolRegular.Gauge24,
                NotificationType.RefreshRate => SymbolRegular.Desktop24,
                NotificationType.TouchpadOn => SymbolRegular.Tablet24,
                NotificationType.TouchpadOff => SymbolRegular.Tablet24,
                NotificationType.WhiteKeyboardBacklight => SymbolRegular.Keyboard24,
                _ => throw new ArgumentException(nameof(notification.Type))
            };

            SymbolRegular? overlaySymbol = notification.Type switch
            {
                NotificationType.CapsLockOff => SymbolRegular.Line24,
                NotificationType.CameraOff => SymbolRegular.Line24,
                NotificationType.FnLockOff => SymbolRegular.Line24,
                NotificationType.MicrophoneOff => SymbolRegular.Line24,
                NotificationType.NumLockOff => SymbolRegular.Line24,
                NotificationType.TouchpadOff => SymbolRegular.Line24,
                _ => null,
            };

            var text = notification.Type switch
            {
                NotificationType.CapsLockOn => Resource.Notification_CapsLockOn,
                NotificationType.CapsLockOff => Resource.Notification_CapsLockOff,
                NotificationType.CameraOn => Resource.Notification_CameraOn,
                NotificationType.CameraOff => Resource.Notification_CameraOff,
                NotificationType.FnLockOn => Resource.Notification_FnLockOn,
                NotificationType.FnLockOff => Resource.Notification_FnLockOff,
                NotificationType.MicrophoneOn => Resource.Notification_MicrophoneOn,
                NotificationType.MicrophoneOff => Resource.Notification_MicrophoneOff,
                NotificationType.NumLockOn => Resource.Notification_NumLockOn,
                NotificationType.NumLockOff => Resource.Notification_NumLockOff,
                NotificationType.PowerMode => string.Format("{0}", notification.Args),
                NotificationType.RefreshRate => string.Format("{0}", notification.Args),
                NotificationType.TouchpadOn => Resource.Notification_TouchpadOn,
                NotificationType.TouchpadOff => Resource.Notification_TouchpadOff,
                NotificationType.WhiteKeyboardBacklight => string.Format(Resource.Notification_WhiteKeyboardBacklight, notification.Args),
                _ => throw new ArgumentException(nameof(notification.Type))
            };

            var closeAfter = notification.Duration switch
            {
                NotificationDuration.Long => 5000,
                _ => 1000,
            };

            ShowNotification(symbol, overlaySymbol, text, closeAfter);
        });

        private void ShowNotification(SymbolRegular symbol, SymbolRegular? overlaySymbol, string text, int closeAfter)
        {
            _window?.Close();

            var nw = new NotificationWindow(symbol, overlaySymbol, text) { Owner = Application.Current.MainWindow };
            nw.Show(closeAfter);
            _window = nw;
        }
    }
}
