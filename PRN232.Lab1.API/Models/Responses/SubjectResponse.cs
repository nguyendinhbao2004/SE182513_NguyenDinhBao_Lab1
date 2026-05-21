using System.Text.Json.Serialization;

namespace PRN232.Lab1.API.Models.Responses
{
    public class SubjectResponse
    {
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int Credit { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyCollection<CourseSummaryResponse>? Courses { get; set; }
    }
}
