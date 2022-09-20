using Remedy.Models;

namespace Remedy.Services.Interfaces
{
    public interface IBTNotificationService
    {
        public Task AddNotificationAsync(Notification notification);
        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);
        public Task<List<Notification>> GetUserNotificationsAsync(string userId);
    }
}
