namespace SAV.domain.Entities.Csv;

public class OrdersCsv
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public int VendedorId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
