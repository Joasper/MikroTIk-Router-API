using MikroClean.Domain.Entities.Base;

public class Router : BaseEntity
{
    public string Name { get; set; }

    public string Ip { get; set; }

    public string User { get; set; }

    public string EncryptedPassword { get; set; }

    public bool IsActive { get; set; }

    public int OrganizationId { get; set; }
}