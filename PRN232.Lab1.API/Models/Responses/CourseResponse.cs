namespace PRN232.Lab1.API.Models.Responses
{
    public class CourseResponse
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public SemesterSummaryResponse? Semester { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public SubjectSummaryResponse? Subject { get; set; }
        public IReadOnlyCollection<EnrollmentSummaryResponse>? Enrollments { get; set; }
    }
}
