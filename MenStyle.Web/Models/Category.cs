using System.ComponentModel.DataAnnotations;

namespace MenStyle.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Filter { get; set; } = "all";

    public bool IsActive { get; set; }
}