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

        [FromQuery(Name = "status")]
        public string? Status { get; set; }

        [FromQuery(Name = "studentId")]
        public int? StudentId { get; set; }

        [FromQuery(Name = "courseId")]
        public int? CourseId { get; set; }

        [FromQuery(Name = "semesterId")]
        public int? SemesterId { get; set; }

        [FromQuery(Name = "subjectId")]
        public int? SubjectId { get; set; }

        [FromQuery(Name = "minCredit")]
        public int? MinCredit { get; set; }

        [FromQuery(Name = "maxCredit")]
        public int? MaxCredit { get; set; }

        [FromQuery(Name = "fromDate")]
        public DateTime? FromDate { get; set; }

        [FromQuery(Name = "toDate")]
        public DateTime? ToDate { get; set; }

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
                Status = Status,
                StudentId = StudentId,
                CourseId = CourseId,
                SemesterId = SemesterId,
                SubjectId = SubjectId,
                MinCredit = MinCredit,
                MaxCredit = MaxCredit,
                FromDate = FromDate,
                ToDate = ToDate,
                Page = Page,
                PageSize = PageSize
            };
        }
    }
}
