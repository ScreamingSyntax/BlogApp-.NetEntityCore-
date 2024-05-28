using Domain.BisleriumBlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BisleriumBlog
{
    public class BlogWithTotalUpVoteandDownVote
    {
            public BlogPost Blog { get; set; }
            public List<BlogReaction> Reactions { get; set; }

            public List<BlogComment> Comments { get; set; }

            public int? Popularity { get; set; }

            public int? TotalUpVote { get; set; }
    public int? TotalDownVote { get;set; }
    }
}
