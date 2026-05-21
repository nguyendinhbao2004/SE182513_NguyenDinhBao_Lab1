namespace PRN232.Lab1.API.Models.Responses
{
    public class SemesterResponse
    {
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IReadOnlyCollection<CourseSummaryResponse>? Courses { get; set; }
    }
}
