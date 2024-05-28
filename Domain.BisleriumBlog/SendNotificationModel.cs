using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.BisleriumBlog
{
    public class SendNotificationModel
    {
        public List<string> TokensOfDevices { set; get; } = new List<string>();
        public string? Message { set; get; }
        public string? Title { set; get; }
        public Boolean? Positivity { set; get; }

    }

}
