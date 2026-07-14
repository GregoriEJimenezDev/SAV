using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.DB_Externa;

[Table("HistoricalSales")]
public class VentasHistoricas
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
