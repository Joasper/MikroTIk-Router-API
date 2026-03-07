<<<<<<< HEAD
﻿using System;
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
=======
﻿using MikroClean.Domain.Entities.Base;

public class Router : BaseEntity
{
    public string Name { get; set; }

    public string Ip { get; set; }

    public string User { get; set; }

    public string EncryptedPassword { get; set; }

    public bool IsActive { get; set; }

    public int OrganizationId { get; set; }
}
>>>>>>> e1056afd14ae74e31ac22d88efc9bbabc0f9e09a
