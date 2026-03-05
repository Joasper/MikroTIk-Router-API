using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class Organizations : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Navigation properties
        public License? License { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Router> Routers { get; set; } = new List<Router>();
    }
}
