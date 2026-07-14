using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SAV.domain.Entities.Data_Warehouse.Dimensions;

namespace SAV.domain.Entities.Data_Warehouse.Facts;

[Table("FactVentas", Schema = "Fact")]
public class FactVentas
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long VentaKey { get; set; }

    [Column("TiempoKey")]
    public int TiempoKey { get; set; }
    [ForeignKey(nameof(TiempoKey))]
    public DimTiempo? DimTiempo { get; set; }

    [Column("ProductoKey")]
    public int ProductoKey { get; set; }
    [ForeignKey(nameof(ProductoKey))]
    public DimProducto? DimProducto { get; set; }

    [Column("ClienteKey")]
    public int ClienteKey { get; set; }
    [ForeignKey(nameof(ClienteKey))]
    public DimCliente? DimCliente { get; set; }

    [Column("VendedorKey")]
    public int VendedorKey { get; set; }
    [ForeignKey(nameof(VendedorKey))]
    public DimVendedor? DimVendedor { get; set; }

    [Column("FuenteKey")]
    public int FuenteKey { get; set; }
    [ForeignKey(nameof(FuenteKey))]
    public DimFuente? DimFuente { get; set; }

    [MaxLength(50)]
    public string FacturaId { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
}
