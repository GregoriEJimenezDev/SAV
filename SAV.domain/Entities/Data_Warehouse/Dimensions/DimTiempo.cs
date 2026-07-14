using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions;

[Table("DimTiempo", Schema = "Dimension")]
public class DimTiempo
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TiempoKey { get; set; }
    public DateTime Fecha { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    [MaxLength(20)]
    public string NombreMes { get; set; } = string.Empty;
    public int Trimestre { get; set; }
    public int Semestre { get; set; }
    public int SemanaAnio { get; set; }
    public int DiaMes { get; set; }
    [MaxLength(20)]
    public string DiaSemana { get; set; } = string.Empty;
    public bool EsFinSemana { get; set; }
    public bool EsFeriado { get; set; }
}
