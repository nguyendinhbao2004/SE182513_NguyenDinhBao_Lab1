using System.Collections.Generic;

namespace PRN232.Lab1.Repositories.Entities
{
    public class Course
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public int SemesterId { get; set; }
        public Semester? Semester { get; set; }
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
