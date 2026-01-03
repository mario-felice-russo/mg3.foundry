using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace mg3.foundry.Features.FoundryCore.Services
{
    public class NotificationService : ObservableObject, IRecipient<NotificationMessage>
    {
        private readonly IMessenger _messenger;

        public ObservableCollection<Notification> Notifications { get; } = new();

        public NotificationService(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.RegisterAll(this);
        }

        public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info, int autoDismissSeconds = 5)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.Now,
                AutoDismissSeconds = autoDismissSeconds
            };

            Notifications.Add(notification);
            _messenger.Send(new NotificationMessage(notification));

            if (autoDismissSeconds > 0)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(autoDismissSeconds * 1000);
                    RemoveNotification(notification.Id);
                });
            }
        }

        public void RemoveNotification(string notificationId)
        {
            var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                Notifications.Remove(notification);
            }
        }

        public void ClearAllNotifications()
        {
            Notifications.Clear();
        }

        public void Receive(NotificationMessage message)
        {
            // Gestione messaggi ricevuti
            if (message.Value is Notification notification)
            {
                if (message.Action == "Dismiss")
                {
                    RemoveNotification(notification.Id);
                }
            }
        }
    }

    public class Notification
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int AutoDismissSeconds { get; set; } = 5;
        public bool IsDismissible { get; set; } = true;
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class NotificationMessage : ValueChangedMessage<Notification>
    {
        public string Action { get; set; } = "Show";

        public NotificationMessage(Notification value, string action = "Show") : base(value)
        {
            Action = action;
        }
    }
}