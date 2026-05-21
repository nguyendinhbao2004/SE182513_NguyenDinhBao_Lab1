namespace PRN232.Lab1.API.Models
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public object? Errors { get; set; }
        public PaginationMetadata? Pagination { get; set; }
    }
}
