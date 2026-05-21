using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.API.Models.Requests
{
    public class EnrollmentRequest
    {
        [Range(1, int.MaxValue)]
        public int StudentId { get; set; }

        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }

        public DateTime? EnrollDate { get; set; }

        [StringLength(30)]
        public string? Status { get; set; }
    }
}
