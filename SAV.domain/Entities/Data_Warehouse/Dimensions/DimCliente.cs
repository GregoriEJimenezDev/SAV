using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions;

[Table("DimCliente", Schema = "Dimension")]
public class DimCliente
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClienteKey { get; set; }
    public int ClienteID { get; set; }
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Pais { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Region { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Segmento { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
    public int IdFuente { get; set; }

    [ForeignKey(nameof(IdFuente))]
    public DimFuente? Fuente { get; set; }
}
