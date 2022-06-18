using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Ledger
    {
        public int Id { get; set; }
        public string TType { get; set; } = "";
        public int VocNo { get; set; }
        public DateTime Date { get; set; }
        public int SrNo { get; set; }
        public int PartyId { get; set; }
        public string? Description { get; set; }
        public Int64? NetDebit { get; set; }
        public Int64? NetCredit { get; set; }
        public bool isDeleted { get; set; } = false;
    }
    public class PersonRow : DataRow
    {
        internal PersonRow(DataRowBuilder builder) : base(builder)
        {
        }
    }

    public class LedgerM
    {
        public int VocNo { get; set; }
        public DateTime Date { get; set; }
        public string TType { get; set; } = "";
        public List<LedgerD>? Trans { get; set; }
    }

    public class LedgerD 
    {
        public int Id { get; set; }
        public int SrNo { get; set; }
        public int PartyId { get; set; }
        public string? Description { get; set; }
        public Int64? NetDebit { get; set; }
        public Int64? NetCredit { get; set; }
        public bool isDeleted { get; set; } = false;
    }
}
