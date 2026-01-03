using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using mg3.foundry.Features.FoundryCore.Services;

namespace mg3.foundry.Features.FoundryCore.ViewModels
{
    public partial class NotificationCenterViewModel : ObservableObject, IRecipient<NotificationMessage>
    {
        private readonly NotificationService _notificationService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private ObservableCollection<Notification> _notifications;

        public NotificationCenterViewModel(NotificationService notificationService, IMessenger messenger)
        {
            _notificationService = notificationService;
            _messenger = messenger;
            _messenger.RegisterAll(this);
            
            Notifications = _notificationService.Notifications;
        }

        [RelayCommand]
        private void Dismiss(string notificationId)
        {
            _notificationService.RemoveNotification(notificationId);
        }

        [RelayCommand]
        private void ClearAll()
        {
            _notificationService.ClearAllNotifications();
        }

        public void Receive(NotificationMessage message)
        {
            // Gestione automatica dei messaggi
            if (message.Action == "Dismiss" && message.Value is Notification notification)
            {
                Dismiss(notification.Id);
            }
        }
    }
}