namespace MenStyle.Web.Models;

public static class ProductCatalog
{
    public static List<Product> GetProducts()
    {
        return
        [
            new Product
            {
                Id = 1,
                CategorySlug = "ao-thun",
                CategoryName = "Áo thun",
                Name = "Áo thun nam basic đen",
                Price = 249000,
                OldPrice = 320000,
                ImageUrl = "/images/product-tshirt.svg",
                AltText = "Áo thun nam basic đen"
            },
            new Product
            {
                Id = 2,
                CategorySlug = "so-mi",
                CategoryName = "Sơ mi",
                Name = "Sơ mi Oxford trắng",
                Price = 399000,
                OldPrice = 450000,
                ImageUrl = "/images/product-shirt.svg",
                AltText = "Sơ mi Oxford trắng"
            },
            new Product
            {
                Id = 3,
                CategorySlug = "quan",
                CategoryName = "Quần jeans",
                Name = "Quần jeans slim fit",
                Price = 459000,
                OldPrice = 520000,
                ImageUrl = "/images/product-jeans.svg",
                AltText = "Quần jeans slim fit"
            },
            new Product
            {
                Id = 4,
                CategorySlug = "ao-khoac",
                CategoryName = "Áo khoác",
                Name = "Áo khoác bomber navy",
                Price = 599000,
                OldPrice = 690000,
                ImageUrl = "/images/product-jacket.svg",
                AltText = "Áo khoác bomber navy"
            },
            new Product
            {
                Id = 5,
                CategorySlug = "quan",
                CategoryName = "Quần kaki",
                Name = "Quần kaki regular fit",
                Price = 379000,
                OldPrice = 430000,
                ImageUrl = "/images/product-kaki.svg",
                AltText = "Quần kaki regular fit"
            },
            new Product
            {
                Id = 6,
                CategorySlug = "ao-thun",
                CategoryName = "Áo polo",
                Name = "Áo polo nam cao cấp",
                Price = 329000,
                OldPrice = 390000,
                ImageUrl = "/images/product-polo.svg",
                AltText = "Áo polo nam cao cấp"
            }
        ];
    }

    public static Product? FindById(int id)
    {
        return GetProducts().FirstOrDefault(p => p.Id == id);
    }
}