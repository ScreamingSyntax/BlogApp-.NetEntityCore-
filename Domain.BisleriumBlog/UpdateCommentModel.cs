﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.BisleriumBlog
{
    public class UpdateCommentModel
    {

        [Required]
        public string? Content { get; set; }
    }
}
