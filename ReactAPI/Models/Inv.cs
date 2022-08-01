namespace WebAPI.Models;

public class Inv
{
    public int Id { get; set; }
    public short TType { get; set; }
    public int VocNo { get; set; }
    public DateTime Date { get; set; }
    public int PartyId { get; set; }
    public string? PartyName { get; set; }
    public int SrNo { get; set; }
    public string? Description { get; set; }
    public long? NetDebit { get; set; }
    public long? NetCredit { get; set; }
    public int? CashAc { get; set; }
    public bool isDeleted { get; set; } = false;
}

public class InvM
{
    public int VocNo { get; set; }
    public DateTime Date { get; set; }
    public string TType { get; set; } = "";
    public int? CashAc { get; set; }
    public List<LedgerD> Trans { get; set; } = new List<LedgerD>();
}

public class InvD
{
    public int Id { get; set; }
    public int SrNo { get; set; }
    public int PartyId { get; set; }
    public string? Description { get; set; }
    public long? NetDebit { get; set; }
    public long? NetCredit { get; set; }
    public string? PartyName { get; set; }
    public bool isDeleted { get; set; } = false;
}
