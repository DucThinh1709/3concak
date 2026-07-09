using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}