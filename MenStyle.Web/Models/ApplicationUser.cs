using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
