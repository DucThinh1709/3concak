using MenStyle.Web.Models;

namespace MenStyle.Web.ViewModels;

public class AdminDashboardViewModel
{
    public List<DashboardMetric> Metrics { get; set; } = [];

    public List<Product> Products { get; set; } = [];

    public List<CustomerOrder> Orders { get; set; } = [];

    public List<string> OrderStatuses { get; set; } = [];
}
