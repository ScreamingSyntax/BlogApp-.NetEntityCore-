using Domain.BisleriumBlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.BisleriumBlog
{
    public interface IReactionService
    {
        Task<BlogReaction>? AddBlogReaction(string blogId, string userId, ReactionType reactionType);
        Task<BlogReaction>? AddCommentReaction(string commentId, string userId, ReactionType reactionType);

    }
}
