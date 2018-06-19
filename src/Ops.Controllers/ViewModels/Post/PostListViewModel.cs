﻿using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Post
{
    public class PostListViewModel
    {
        public IEnumerable<Models.Post> Posts { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}