using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions;

[Table("DimVendedor", Schema = "Dimension")]
public class DimVendedor
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VendedorKey { get; set; }
    public int VendedorID { get; set; }
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Region { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
    public int IdFuente { get; set; }

    [ForeignKey(nameof(IdFuente))]
    public DimFuente? Fuente { get; set; }
}
