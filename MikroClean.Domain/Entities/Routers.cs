using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    internal class Routers:BaseEntity
    {
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public int Organization_Id { get; set; }
        public string User { get; set; }
        public string EncryptedPassword { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
