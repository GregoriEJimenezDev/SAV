using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions;

[Table("DimProducto", Schema = "Dimension")]
public class DimProducto
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductoKey { get; set; }
    public int ProductoID { get; set; }
    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string Categoria { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioActual { get; set; }
    public int Stock { get; set; }
    [MaxLength(100)]
    public string Marca { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
    public int IdFuente { get; set; }

    [ForeignKey(nameof(IdFuente))]
    public DimFuente? Fuente { get; set; }
}
