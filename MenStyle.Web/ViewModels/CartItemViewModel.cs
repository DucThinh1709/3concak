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

    public int StockQuantity { get; set; }

    public int TotalProductQuantityInCart { get; set; }

    public bool IsProductActive { get; set; }

    public bool HasStockIssue =>
        !IsProductActive
        || StockQuantity <= 0
        || Quantity <= 0
        || TotalProductQuantityInCart > StockQuantity;

    public bool CanIncrease =>
        IsProductActive
        && StockQuantity > 0
        && TotalProductQuantityInCart < StockQuantity;

    public decimal LineTotal => Price * Quantity;
}
