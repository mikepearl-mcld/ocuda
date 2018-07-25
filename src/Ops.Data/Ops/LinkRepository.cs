﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class LinkRepository 
        : GenericRepository<Models.Link, int>, ILinkRepository
    {
        public LinkRepository(OpsContext context, ILogger<LinkRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<Link> GetByNameAndSectionIdAsync(string name, int sectionId)
        {
            return await DbSet
                    .AsNoTracking()
                    .Where(_ => string.Equals(_.Name, name, StringComparison.OrdinalIgnoreCase)
                             && _.SectionId == sectionId)
                    .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(_ => _.CategoryId == filter.CategoryId);
            }
            else if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId);
            }

            return new DataWithCount<ICollection<Link>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
