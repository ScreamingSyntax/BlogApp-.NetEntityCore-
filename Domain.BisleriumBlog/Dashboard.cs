using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Domain.BisleriumBlog
{
    public class Dashboard
    {
        public int BlogCount { get; set; }
        public int CommentCount { get; set; }
        public int? UpvoteCount { get; set; }
        public int? DownvoteCount { get; set; }
        public List<AppUser>? TopTenUser { get; set; }
        public List<BlogPost>? TopTenBlog { get; set; }
        public List<BlogCountByMonth>? BlogCountByMonth { get; set; }


    }

    public class BlogCountByMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
    }

}
