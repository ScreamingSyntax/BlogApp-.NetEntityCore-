using Application.BisleriumBlog;
using Domain.BisleriumBlog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BisleriumBlog
{
    public class BlogCommentService : IBlogCommentService
    {
        private readonly AplicationDBContext _context;
        private readonly NotificationServices _notificationService;
        public BlogCommentService(AplicationDBContext context, NotificationServices notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        public async Task<BlogComment> AddBlogComment(BlogComment blogComment)
        {
            var result = await _context.BlogComments.AddAsync(blogComment);
            await _context.SaveChangesAsync();
            await NotifyAuthor(blogComment);
            return result.Entity;
        }

        private async Task NotifyAuthor(BlogComment blogComment)
        {
            var blogPost = await _context.BlogPosts.FindAsync(blogComment.BlogPostId);
            if (blogPost != null)
            {
                var tokens = await _notificationService.GetDeviceTokensByUserId(blogPost.AuthorId.ToString());
               
                    var notification = new SendNotificationModel
                    {
                        Title = "New Comment on Your Post",
                        Message = $"A new comment has been added to your post: '{blogPost.Title}'",
                        TokensOfDevices = tokens
                    };

                    await _notificationService.sendNotification(notification);
            }
        }

        public async Task<BlogComment> UpdateBlogComment(BlogComment blogComment)
        {
            var existingComment = await _context.BlogComments.FindAsync(blogComment.Id);
            if (existingComment == null)
                throw new ArgumentException("Comment not found.");

            var commentHistory = new CommentHistory
            {
                Id = Guid.NewGuid(),
                BlogPostId = existingComment.BlogPostId,
                AuthorId = existingComment.AuthorId,
                Content = existingComment.Content,
                CommentUpdateDate = DateTime.UtcNow
            };

            await _context.CommentHistory.AddAsync(commentHistory);

            existingComment.Content = blogComment.Content;
            _context.BlogComments.Update(existingComment);
            await _context.SaveChangesAsync();
            return existingComment;
        }


        public async Task<bool> DeleteBlogComment(string commentId)
        {
            Guid commentGuid;
            if (!Guid.TryParse(commentId, out commentGuid))
            {
                return false;
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var comment = await _context.BlogComments.FindAsync(commentGuid);
                    if (comment == null)
                        return false;

                    var commentReactions = await _context.BlogReactions
                                                         .Where(br => br.CommentId == comment.Id)
                                                         .ToListAsync();
                    if (commentReactions.Any())
                    {
                        _context.BlogReactions.RemoveRange(commentReactions);
                    }

                    _context.BlogComments.Remove(comment);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Error deleting blog comment: " + ex.Message);
                    return false;
                }
            }
        }


        public async Task<IEnumerable<BlogCommentWithReactions>> GetBlogComments(string blogId)
        {
            var comments = await _context.BlogComments
                .Where(c => c.BlogPostId.ToString() == blogId)
                .Include(c => c.Author)
                .ToListAsync();

            var commentsWithReactions = comments.Select(c => new BlogCommentWithReactions
            {
                Id = c.Id,
                BlogPostId = c.BlogPostId,
                AuthorId = c.AuthorId,
                Content = c.Content,
                CommentDate = c.CommentDate,
                BlogPost = c.BlogPost,
                Author = c.Author,
                Reactions = _context.BlogReactions.Where(r => r.CommentId == c.Id).ToList()
            });

            return commentsWithReactions.Any() ? commentsWithReactions : new BlogCommentWithReactions[0];
        }

        public async Task<IEnumerable<BlogComment>> GetBlogCommentById(string blogId)
        {
            var comments = await _context.BlogComments.Where(c => c.BlogPostId.ToString() == blogId).ToListAsync();
            return comments.Any() ? comments : new BlogComment[0];
        }
        public async Task<BlogComment> GetCommentById(string commentId)
        {
            var comments = await _context.BlogComments.Where(c => c.Id.ToString() == commentId).ToListAsync();
            return comments![0];
        }

        public async Task<IEnumerable<CommentHistory>> GetBlogCommentHistoryByUserID(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            Guid userGuid;
            if (!Guid.TryParse(userId, out userGuid))
            {
                throw new ArgumentException("Invalid user ID format.", nameof(userId));
            }

            var history = await _context.CommentHistory
                .Where(ch => ch.AuthorId == userGuid)
                .OrderByDescending(ch => ch.CommentUpdateDate) 
                .ToListAsync();

            return history;
        }

    }
}
