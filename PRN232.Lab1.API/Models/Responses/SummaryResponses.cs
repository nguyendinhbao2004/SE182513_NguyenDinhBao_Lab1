namespace PRN232.Lab1.API.Models.Responses
{
    public class StudentSummaryResponse
    {
        public int StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class SemesterSummaryResponse
    {
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SubjectSummaryResponse
    {
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int Credit { get; set; }
    }

    public class CourseSummaryResponse
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
    }

    public class EnrollmentSummaryResponse
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public DateTime EnrollDate { get; set; }
        public string? Status { get; set; }
    }
}
