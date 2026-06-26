using MenStyle.Web.Models;

namespace MenStyle.Web.ViewModels;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = [];
    public List<Product> Products { get; set; } = [];
    public List<DashboardMetric> Metrics { get; set; } = [];
    public List<OrderSummary> RecentOrders { get; set; } = [];
}
