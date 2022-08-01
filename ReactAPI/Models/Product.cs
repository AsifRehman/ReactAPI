
namespace WebAPI.Models;

public class Product
{
    public int PartyId { get; set; }
    public string PartyName { get; set; } = "";
    public Int64? Debit { get; set; }
    public Int64? Credit { get; set; }
    public int PartyTypeId { get; set; }

}
