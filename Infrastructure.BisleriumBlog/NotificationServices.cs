using Application.BisleriumBlog;
using Domain.BisleriumBlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BisleriumBlog
{
    public class NotificationServices
    {
        private readonly AplicationDBContext? _dbContext;
        public NotificationServices(AplicationDBContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<MobileTokens> SaveToken(MobileTokens token)
        {
            try
            {
                var tokenCheck = _dbContext!.MobileTokens.FirstOrDefault(e => e.Token == token.Token);
                if (tokenCheck != null)
                {
                    return tokenCheck;
                }
                else
                {
                    var result = await _dbContext.MobileTokens.AddAsync(token);
                    await _dbContext.SaveChangesAsync(); 
                    return result.Entity; 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public Task<List<string>> GetDeviceTokensByUserId(string? userid)
        {
            try
            {
                if (userid != null)
                {
                    List<string> tokens = _dbContext.MobileTokens
                        .Where(e => e.UserId == userid)
                        .Select(e => e.Token)
                        .ToList();
                    return Task.FromResult(tokens);
                }
                else
                {
                    List<string> tokens = _dbContext.MobileTokens
                        .Select(e => e.Token)
                        .ToList();
                    return Task.FromResult(tokens);
                }
            }
            catch
            {
                return Task.FromResult(new List<string>());
            }
        }

        public async Task<bool> sendNotification(SendNotificationModel? token)
        {


            var url = "https://fcm.googleapis.com/fcm/send";

            var data = new
            {
                data = new { imageurl = "Hi" },
                notification = new
                {
                    title = token.Title,
                    body =
                    token.Message,
                    mutable_content = true
                },
                registration_ids = token.TokensOfDevices
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=AAAAHULtPTE:APA91bGVs6I4el4Ew8QGQwpzQKChYoIRyhUwo4Dzb7YdtmW0u5KCEMdv2ZVsFiS91xYQFxmDKJPqKQtopn03eVbpxX--nXfYimbqY8pNlwhGMG2GQr-ejVmgIEYCDS2aZM7hZnNWjSVF");

            var response = await client.PostAsync(url, content);

            Console.WriteLine("Successfully sent notification: " + response.StatusCode);

            return false;

        }

    }

}
