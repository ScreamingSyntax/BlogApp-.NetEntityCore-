using Application.BisleriumBlog;
using Domain.BisleriumBlog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.BisleriumBlog
{
    public class BlogReactionService : IReactionService
    {
        private readonly AplicationDBContext _context;
        private readonly NotificationServices _notificationServices;
        public BlogReactionService(AplicationDBContext context, NotificationServices notificationServices)
        {
            _context = context;
            _notificationServices = notificationServices;
        }
        public async Task<BlogReaction>? AddBlogReaction(string blogId, string userId, ReactionType reactionType)
        {
            try
            {
                var blogPost = await _context.BlogPosts.Include(bp => bp.Author).FirstOrDefaultAsync(bp => bp.Id == Guid.Parse(blogId));
                if (blogPost == null) return null;

                var existingReaction = await _context.BlogReactions
                   .FirstOrDefaultAsync(r => r.BlogPostId == blogPost.Id && r.UserId == Guid.Parse(userId));

                string notificationMessage = "";
                BlogReaction reactionResult = null;

                if (existingReaction != null)
                {
                    if (existingReaction.Type == reactionType)
                    {
                        _context.BlogReactions.Remove(existingReaction);
                        notificationMessage = $"Your post has had a {reactionType.ToString().ToLower()} removed.";
                    }
                    else
                    {
                        existingReaction.Type = reactionType;
                        reactionResult = existingReaction;
                        notificationMessage = $"Your post has been {reactionType.ToString().ToLower()}d.";
                    }
                }
                else
                {
                    var newReaction = new BlogReaction
                    {
                        BlogPostId = blogPost.Id,
                        UserId = Guid.Parse(userId),
                        Type = reactionType,
                        ReactionDate = DateTime.Now
                    };

                    _context.BlogReactions.Add(newReaction);
                    reactionResult = newReaction;
                    notificationMessage = $"Your post has been {reactionType.ToString().ToLower()}d.";
                }

                await _context.SaveChangesAsync();

                if (reactionResult != null || existingReaction != null) 
                {
                    var deviceTokens = await _notificationServices.GetDeviceTokensByUserId(blogPost.AuthorId.ToString());
                    if (deviceTokens.Any()) 
                    {
                        var notificationModel = new SendNotificationModel
                        {
                            TokensOfDevices = deviceTokens,
                            Message = notificationMessage,
                            Title = notificationMessage
                        };
                        await _notificationServices.sendNotification(notificationModel);
                    }
                }

                return reactionResult;
            }
            catch (Exception ex)
            {
                throw;  // Consider logging the exception or handling it more gracefully
            }
        }



        public async Task<BlogReaction>? AddCommentReaction(string commentId, string userId, ReactionType reactionType)
        {
            try
            {
                var comment = await _context.BlogComments.Include(c => c.Author).FirstOrDefaultAsync(c => c.Id == Guid.Parse(commentId));
                if (comment == null) return null;  // Ensure comment exists

                var existingReaction = await _context.BlogReactions
                    .FirstOrDefaultAsync(r => r.CommentId == comment.Id && r.UserId == Guid.Parse(userId));

                string notificationMessage = "";
                BlogReaction reactionResult = null;

                if (existingReaction != null)
                {
                    if (existingReaction.Type == reactionType)
                    {
                        _context.BlogReactions.Remove(existingReaction);
                        notificationMessage = $"Someone has removed their {reactionType.ToString().ToLower()} from your comment.";
                    }
                    else
                    {
                        existingReaction.Type = reactionType;
                        reactionResult = existingReaction;
                        notificationMessage = $"Someone has {reactionType.ToString().ToLower()}d your comment.";
                    }
                }
                else
                {
                    var newReaction = new BlogReaction
                    {
                        CommentId = comment.Id,
                        UserId = Guid.Parse(userId),
                        Type = reactionType,
                        ReactionDate = DateTime.Now
                    };

                    _context.BlogReactions.Add(newReaction);
                    reactionResult = newReaction;
                    notificationMessage = $"Someone has {reactionType.ToString().ToLower()}d your comment.";
                }

                await _context.SaveChangesAsync();

                if ((reactionResult != null || existingReaction != null)) 
                {
                    var deviceTokens = await _notificationServices.GetDeviceTokensByUserId(comment.AuthorId.ToString());
                    if (deviceTokens.Any())  
                    {
                        var notificationModel = new SendNotificationModel
                        {
                            TokensOfDevices = deviceTokens,
                            Message = notificationMessage,
                            Title =  notificationMessage
                        };
                        await _notificationServices.sendNotification(notificationModel);
                    }
                }

                return reactionResult;
            }
            catch (Exception ex)
            {
                throw; 
            }
        }



    }
}