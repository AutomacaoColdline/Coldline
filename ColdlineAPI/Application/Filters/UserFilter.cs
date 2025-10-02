using System;
using System.Collections.Generic;


namespace ColdlineAPI.Application.Filters
{
    public class UserFilter
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
