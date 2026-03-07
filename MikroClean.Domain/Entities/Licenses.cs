using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class Licenses:BaseEntity  
    {
        public string LicenseKey { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string type { get; set; }
        public int Organization { get; set; } 
    }
}
