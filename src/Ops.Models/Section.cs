﻿using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class Section : Abstract.BaseEntity
    {
        [MaxLength(64)]
        public string Path { get; set; }
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
        [MaxLength(32)]
        public string Icon { get; set; }
        public int SortOrder { get; set; }

        public int PostsToShow { get; set; }
        public int LinksToShow { get; set; }
        public int ItemsToShow { get; set; }
        public int CalendarItemsToShow { get; set; }
    }
}