using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class Device : BaseEntity
    {
        public string Name { get; set; }
        public string Model { get; set; }
    }
}
