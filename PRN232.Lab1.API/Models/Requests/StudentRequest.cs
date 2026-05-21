using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.API.Models.Requests
{
    public class StudentRequest
    {
        [Required]
        [StringLength(150)]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
    }
}
