using System;

namespace PRN232.Lab1.Services.Models
{
    public class EnrollmentModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public StudentModel? Student { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public CourseModel? Course { get; set; }
        public DateTime EnrollDate { get; set; }
        public string? Status { get; set; }
    }
}
