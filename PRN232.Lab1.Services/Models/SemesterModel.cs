namespace PRN232.Lab1.Services.Models
{
    public class SemesterModel
    {
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IReadOnlyCollection<CourseModel>? Courses { get; set; }
    }
}
