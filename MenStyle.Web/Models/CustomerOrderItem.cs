using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenStyle.Web.Models;

public class CustomerOrderItem
{
    public int Id { get; set; }

    public int CustomerOrderId { get; set; }

    public CustomerOrder? CustomerOrder { get; set; }

    public int ProductId { get; set; }

    [Required]
    [StringLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
}