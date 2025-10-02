using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Models.Filter
{
    public class UserTypeFilterModel
    {
        public string? Name { get; set; }
        public string? DepartmentId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }  
        public bool SortDesc { get; set; } = true;


    }
}
