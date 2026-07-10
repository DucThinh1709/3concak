using MenStyle.Web.Models;

namespace MenStyle.Web.ViewModels;

public class CollectionPageViewModel
{
    public List<CollectionItemViewModel> Collections { get; set; } = [];

    public List<Product> FeaturedProducts { get; set; } = [];

    public int TotalProducts { get; set; }
}

public class CollectionItemViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<Product> Products { get; set; } = [];
}