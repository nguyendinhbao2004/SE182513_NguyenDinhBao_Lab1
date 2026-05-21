using System;

namespace PRN232.Lab1.Repositories.Entities
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public DateTime EnrollDate { get; set; }
        public string? Status { get; set; }
    }
}
