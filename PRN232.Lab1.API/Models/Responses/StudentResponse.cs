using System;
using System.Text.Json.Serialization;

namespace PRN232.Lab1.API.Models.Responses
{
    public class StudentResponse
    {
        public int StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyCollection<EnrollmentSummaryResponse>? Enrollments { get; set; }
    }
}
