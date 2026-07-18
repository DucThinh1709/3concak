using MenStyle.Web.Models;

namespace MenStyle.Web.ViewModels;

public class ProductDetailViewModel
{
    public Product Product { get; set; } = new();

    public List<Product> RelatedProducts { get; set; } = [];

    public List<string> Sizes { get; set; } = [];

    public List<string> Colors { get; set; } = [];

    public Dictionary<string, string> ColorImages { get; set; } = [];
}