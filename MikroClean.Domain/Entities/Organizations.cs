using MikroClean.Domain.Entities.Base;
using System.Numerics;

namespace MikroClean.Domain.Entities
{
    public class Organizations : BaseEntity
    {
        public string Name { get; set; }
        public string Contact_email { get; set; }
        public string Phone { get; set; }

    }
}
