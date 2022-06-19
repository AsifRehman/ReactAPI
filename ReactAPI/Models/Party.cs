
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Party
    {
        public int PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public int Debit { get; set; }
        public int Credit { get; set; }
        public int PartyTypeId { get; set; }

    }
}
