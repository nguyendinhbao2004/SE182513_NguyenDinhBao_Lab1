using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.API.Models.Requests
{
    public class SemesterRequest
    {
        [Required]
        [StringLength(100)]
        public string? SemesterName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
