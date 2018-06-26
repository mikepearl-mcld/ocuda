﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface IPageRepository : IRepository<Page, int>
    {
        Task<Page> GetByStubAsync(string stub);
        Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BlogFilter filter);
    }
}