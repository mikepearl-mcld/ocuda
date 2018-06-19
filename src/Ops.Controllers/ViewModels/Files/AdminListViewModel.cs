﻿using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Files
{
    public class AdminListViewModel
    {
        public IEnumerable<File> Files { get; set; }
        public File File { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}