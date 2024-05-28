using Application.BisleriumBlog;
using Domain.BisleriumBlog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BisleriumBlog
{
    public class AdminServices : IAdminServices
    {
        private readonly AplicationDBContext _context;
        private readonly IBlogPostService _blogServices;
        public AdminServices(IBlogPostService blogServices, AplicationDBContext context, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _blogServices = blogServices;


        }

        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private Dashboard countView;

        public async Task<AdminCreateModel> CreateAdminAsync(AdminCreateModel adminModel)
        {
            AppUser user = new AppUser
            {
                Email = adminModel.Email,
                UserName = adminModel.Email
            };
            Console.WriteLine(user);

            var result = await _userManager.CreateAsync(user, adminModel.Password);
            Console.WriteLine(result);
            if (result.Succeeded)
            {
                //await _userManager.AddToRoleAsync(user, model.Role);
                await _userManager.AddToRoleAsync(user, "Admin");

                return adminModel;
                //return Ok("User registered successfully.");
            }
            throw new NotImplementedException();
        }



        public async Task<Dashboard> getDashboardDetails(int months)
        {
            var dashboard = new Dashboard();

            var blogsQuery = _context.BlogPosts
                .Where(bp => !bp.isDeleted)
                .Include(bp => bp.Author)
                .Select(bp => new
                {
                    Blog = bp,
                    Reactions = _context.BlogReactions.Where(br => br.BlogPostId == bp.Id).ToList(),
                    Comments = _context.BlogComments.Where(bc => bc.BlogPostId == bp.Id).ToList(),
                    Popularity = 2 * _context.BlogReactions.Count(br => br.BlogPostId == bp.Id && br.Type == ReactionType.UpVote) -
                                 _context.BlogReactions.Count(br => br.BlogPostId == bp.Id && br.Type == ReactionType.DownVote) +
                                 _context.BlogComments.Count(bc => bc.BlogPostId == bp.Id)
                })
                .AsQueryable();

            if (months != 0)
            {
                var startDate = new DateTime(2024, months, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                blogsQuery = blogsQuery.Where(b => b.Blog.PostDate >= startDate && b.Blog.PostDate <= endDate);
            }

            var blogs = await blogsQuery.ToListAsync();

            dashboard.BlogCount = blogs.Count;
            dashboard.CommentCount = blogs.Sum(b => b.Comments.Count);
            dashboard.UpvoteCount = blogs.Sum(b => b.Reactions.Count(r => r.Type == ReactionType.UpVote));
            dashboard.DownvoteCount = blogs.Sum(b => b.Reactions.Count(r => r.Type == ReactionType.DownVote));

            dashboard.TopTenBlog = blogs
                .OrderByDescending(b => b.Popularity)
                .Take(10)
                .Select(b => b.Blog)
                .ToList();

            dashboard.TopTenUser = blogs
                .GroupBy(b => b.Blog.AuthorId)
                .Select(g => new
                {
                    AuthorId = g.Key,
                    TotalPopularity = g.Sum(x => x.Popularity),
                    Author = g.First().Blog.Author
                })
                .OrderByDescending(g => g.TotalPopularity)
                .Take(10)
                .Select(g => g.Author)
                .ToList()!;

            return dashboard;
        }

        public async Task<IEnumerable<BlogPost>> getBlogByMonth(int month)
        {
            var startDate = new DateTime(2024, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var blogs = await _context.BlogPosts
                .Where(b => b.PostDate >= startDate && b.PostDate <= endDate)
                .ToListAsync();

            return blogs;
        }

   
    }
}
