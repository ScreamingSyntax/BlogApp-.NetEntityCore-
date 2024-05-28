using Domain.BisleriumBlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.BisleriumBlog
{
    public interface INotificationServices
    {
        Task<MobileTokens> SaveToken(MobileTokens token);
        Task<bool> sendNotification(SendNotificationModel token);
        Task<List<string>> GetDeviceTokensByUserId(string? userid);
    }

}
