using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.Services.Models;
using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.API.Models.Requests
{
    public class QueryParameters
    {
        [FromQuery(Name = "search")]
        public string? Search { get; set; }

        [FromQuery(Name = "sort")]
        public string? Sort { get; set; }

        [FromQuery(Name = "fields")]
        public string? Fields { get; set; }

        [FromQuery(Name = "expand")]
        public string? Expand { get; set; }

        [Range(1, int.MaxValue)]
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 10;

        public QueryOptions ToOptions()
        {
            return new QueryOptions
            {
                Search = Search,
                Sort = Sort,
                Fields = Fields,
                Expand = Expand,
                Page = Page,
                PageSize = PageSize
            };
        }
    }
}
