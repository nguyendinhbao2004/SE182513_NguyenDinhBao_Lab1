using System.ComponentModel.DataAnnotations;

namespace PRN232.Lab1.API.Models.Requests
{
    public class SubjectRequest
    {
        [Required]
        [StringLength(20)]
        public string? SubjectCode { get; set; }

        [Required]
        [StringLength(150)]
        public string? SubjectName { get; set; }

        [Range(1, 10)]
        public int Credit { get; set; }
    }
}
