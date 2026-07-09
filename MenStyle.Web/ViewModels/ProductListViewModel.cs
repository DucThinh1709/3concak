using MenStyle.Web.Models;

namespace MenStyle.Web.ViewModels;

public class ProductListViewModel
{
    public List<Category> Categories { get; set; } = [];
    public List<Product> Products { get; set; } = [];

    public string SelectedCategory { get; set; } = "all";
    public string SortOrder { get; set; } = "default";

    public string? Keyword { get; set; }
}