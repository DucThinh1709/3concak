namespace MenStyle.Web.Models;

public class Category
{
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Filter { get; set; } = "all";
    public bool IsActive { get; set; }
}
