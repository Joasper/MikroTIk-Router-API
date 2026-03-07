using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    namespace MikroClean.Domain.Entities
    {
        public class Notifications : BaseEntity
        {


            public string Module { get; set; }

            public string Message { get; set; }

            public int UserId { get; set; }

            public bool IsViewed { get; set; }





        }
    }



}
