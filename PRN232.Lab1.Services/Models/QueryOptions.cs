namespace PRN232.Lab1.Services.Models
{
    public class QueryOptions
    {
        public string? Search { get; set; }
        public string? Sort { get; set; }
        public string? Fields { get; set; }
        public string? Expand { get; set; }
        public string? Status { get; set; }
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
        public int? SemesterId { get; set; }
        public int? SubjectId { get; set; }
        public int? MinCredit { get; set; }
        public int? MaxCredit { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
