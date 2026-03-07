using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    internal class Users: BaseEntity
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int Organization_Id { get; set; }
        public int System_Role_Id { get; set; }
        public DateTime CreatedAat { get; set; }

    }
}
