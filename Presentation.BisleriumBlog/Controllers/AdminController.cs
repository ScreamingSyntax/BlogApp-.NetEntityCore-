using Application.BisleriumBlog;
using Domain.BisleriumBlog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.BisleriumBlog.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;

        private readonly IAdminServices _adminServices;

        public AdminController(Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager, IAdminServices adminServices)
        {
            _userManager = userManager;
            _adminServices = adminServices;

        }

        [HttpPost("CreateNewAdmin")]

        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Console.WriteLine(model);


            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
            };


            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Ok("User registered successfully.");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("CreateAdmin")]
        public async Task<IActionResult> CreateAdmin([FromForm] AdminCreateModel adminModel)
        {
            Console.WriteLine(adminModel);
            var result = await _adminServices.CreateAdminAsync(adminModel);

            return Ok(result);

        }

        [HttpGet("GetDashBoard")]
        public async Task<IActionResult> GetDashBoardDetails(int month)
        {
            var getTotalCount = await _adminServices.getDashboardDetails(month);

            return Ok(new { getTotalCount, Message = "Counts displayed Successfully." });
        }

        [HttpGet("FilterByMonth")]

        public async Task<IActionResult> GetBlogByMonths(int month)
        {
            var getTotalCount = await _adminServices.getDashboardDetails(month);

            return Ok(new { getTotalCount, Message = "Counts displayed Successfully." });
        }


    }
}
