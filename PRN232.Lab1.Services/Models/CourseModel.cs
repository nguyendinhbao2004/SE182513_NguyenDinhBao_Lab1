namespace PRN232.Lab1.Services.Models
{
    public class CourseModel
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public SemesterModel? Semester { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public SubjectModel? Subject { get; set; }
        public IReadOnlyCollection<EnrollmentModel>? Enrollments { get; set; }
    }
}
