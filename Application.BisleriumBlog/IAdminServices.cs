using Domain.BisleriumBlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.BisleriumBlog
{
    public interface IAdminServices
    {
        Task<AdminCreateModel> CreateAdminAsync(AdminCreateModel adminModel);
        Task<Dashboard> getDashboardDetails(int months);
    }

}
