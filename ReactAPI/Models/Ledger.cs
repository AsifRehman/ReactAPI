namespace WebAPI.Models;

public class Ledger
{
    public int Id { get; set; }
    public string TType { get; set; } = "";
    public int VocNo { get; set; }
    public DateTime Date { get; set; }
    public int SrNo { get; set; }
    public int PartyId { get; set; }
    public string? PartyName { get; set; }
    public string? Description { get; set; }
    public Int64? NetDebit { get; set; }
    public Int64? NetCredit { get; set; }
    public int? CashAc { get; set; }
    public bool isDeleted { get; set; } = false;
}

public class LedgerM
{
    public int VocNo { get; set; }
    public DateTime Date { get; set; }
    public string TType { get; set; } = "";
    public int? CashAc { get; set; }
    public List<LedgerD> Trans { get; set; } = new List<LedgerD>();
}

public class LedgerD
{
    public int Id { get; set; }
    public int SrNo { get; set; }
    public int PartyId { get; set; }
    public string? Description { get; set; }
    public Int64? NetDebit { get; set; }
    public Int64? NetCredit { get; set; }
    public string? PartyName { get; set; }
    public bool isDeleted { get; set; } = false;
}

