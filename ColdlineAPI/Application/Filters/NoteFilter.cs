using System;
using System.Collections.Generic;
using ColdlineAPI.Domain.Enum;

namespace ColdlineAPI.Application.Filters
{
    public class NoteFilter
    {
        public string? name { get; set; }
        public string? element { get; set; }
        public NoteType? noteType { get; set; }

    }
}
