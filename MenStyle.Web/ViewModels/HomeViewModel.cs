using MenStyle.Web.Models;

namespace MenStyle.Web.ViewModels;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = [];
    public List<Product> Products { get; set; } = [];
    public Product? HeroProduct { get; set; }

    public List<Product> IntroProducts { get; set; } = [];

    public List<Product> SliderProducts { get; set; } = [];

    public List<Product> HeroProducts { get; set; } = [];

}
