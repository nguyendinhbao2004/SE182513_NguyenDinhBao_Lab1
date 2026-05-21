namespace PRN232.Lab1.Repositories.Entities
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int Credit { get; set; }

        public ICollection<Course>? Courses { get; set; }
    }
}
