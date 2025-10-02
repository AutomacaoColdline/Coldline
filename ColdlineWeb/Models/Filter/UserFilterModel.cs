using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Models.Filter
{
    public class UserFilterModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? DepartmentId { get; set; }
        public string? UserTypeId { get; set; }

        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? SortBy { get; set; }      
        public bool? SortDesc { get; set; }


    }
}


