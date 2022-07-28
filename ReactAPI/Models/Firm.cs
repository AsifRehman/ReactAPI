
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Firm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "Address";
        public string Email { get; set; } = "Email";
        public string Mobile { get; set; } = "Mobile";

    }
}
