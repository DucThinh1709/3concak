namespace MenStyle.Web.Models;

public class OrderSummary
{
    public string Code { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Total { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
}
