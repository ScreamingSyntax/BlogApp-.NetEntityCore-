using Domain.BisleriumBlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.BisleriumBlog
{
    public interface IBlogCommentService
    {
        Task<BlogComment> AddBlogComment(BlogComment blogPost);
        Task<BlogComment> UpdateBlogComment(BlogComment blogPost);
        Task<bool> DeleteBlogComment(String commentId);
        Task<IEnumerable<BlogCommentWithReactions>> GetBlogComments(String blogId);
        Task<IEnumerable<BlogComment>> GetBlogCommentById(String blogId);
        Task<IEnumerable<CommentHistory>> GetBlogCommentHistoryByUserID(String userId);

        Task<BlogComment> GetCommentById(string commentId);
    }
}
