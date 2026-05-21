using System;
using System.Text.Json.Serialization;

namespace PRN232.Lab1.API.Models.Responses
{
    public class EnrollmentResponse
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StudentSummaryResponse? Student { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CourseSummaryResponse? Course { get; set; }
        public DateTime EnrollDate { get; set; }
        public string? Status { get; set; }
    }
}
