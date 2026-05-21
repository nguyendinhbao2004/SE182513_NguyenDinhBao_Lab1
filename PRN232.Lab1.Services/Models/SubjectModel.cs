namespace PRN232.Lab1.Services.Models
{
    public class SubjectModel
    {
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int Credit { get; set; }
        public IReadOnlyCollection<CourseModel>? Courses { get; set; }
    }
}
