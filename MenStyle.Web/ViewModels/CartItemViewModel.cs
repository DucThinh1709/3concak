namespace MenStyle.Web.ViewModels;

public class CartItemViewModel
{
    public int CartItemId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string SelectedSize { get; set; } = string.Empty;

    public string SelectedColor { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal => Price * Quantity;
}