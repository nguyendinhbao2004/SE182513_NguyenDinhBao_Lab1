using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.API.Models.Requests
{
    public class CourseRequest
    {
        [Required]
        [StringLength(150)]
        public string? CourseName { get; set; }

        [Range(1, int.MaxValue)]
        public int SemesterId { get; set; }

        [Range(1, int.MaxValue)]
        public int SubjectId { get; set; }
    }
}
