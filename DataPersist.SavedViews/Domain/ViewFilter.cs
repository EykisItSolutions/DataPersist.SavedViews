﻿using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class ViewFilter
    {
        public int Id { get; set; }
        public int ViewId { get; set; }
        public int Number { get; set; }
        public string Column { get; set; } = null!;
        public string Operator { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Clause { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual View View { get; set; } = null!;
    }
}
