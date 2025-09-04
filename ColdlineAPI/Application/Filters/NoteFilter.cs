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

        // paginação
        public int? page { get; set; }       // default 1
        public int? pageSize { get; set; }   // default 20 (limite que você preferir)

        // ordenação (opcional): "name", "noteType"
        public string? sortBy { get; set; }
        public bool? sortDesc { get; set; }

    }
}
