namespace PRN232.Lab1.Services.Models
{
    public class QueryOptions
    {
        public string? Search { get; set; }
        public string? Sort { get; set; }
        public string? Fields { get; set; }
        public string? Expand { get; set; }
  
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
