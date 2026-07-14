using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions;

[Table("DimFuente", Schema = "Dimension")]
public class DimFuente
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int FuenteKey { get; set; }
    public int IdFuente { get; set; }
    [Required, MaxLength(50)]
    public string NombreFuente { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string Tipo { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
}
