namespace MenStyle.Web.ViewModels;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = [];

    public decimal Total => Items.Sum(x => x.LineTotal);

    public int TotalQuantity => Items.Sum(x => x.Quantity);

    public bool HasStockIssues => Items.Any(x => x.HasStockIssue);

    public bool CanCheckout => Items.Any() && !HasStockIssues;
}
