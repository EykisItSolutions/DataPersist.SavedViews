using System;
using System.Collections.Generic;

namespace DataPersist.SavedViews.Domain
{
    public partial class View
    {
        public View()
        {
            ViewFilters = new HashSet<ViewFilter>();
            ViewSorts = new HashSet<ViewSort>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string What { get; set; } = null!;
        public string? Parms { get; set; }
        public string? FilterLogic { get; set; }
        public string? FilterClause { get; set; }
        public string? SortClause { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

        public virtual ICollection<ViewFilter> ViewFilters { get; set; }
        public virtual ICollection<ViewSort> ViewSorts { get; set; }
    }
}
