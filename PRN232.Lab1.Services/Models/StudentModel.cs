using System;

namespace PRN232.Lab1.Services.Models
{
    public class StudentModel
    {
        public int StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public IReadOnlyCollection<EnrollmentModel>? Enrollments { get; set; }
    }
}
