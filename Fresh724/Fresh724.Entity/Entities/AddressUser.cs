using Fresh724.Core.Entities;

namespace Fresh724.Entity.Entities;

public class AddressUser:AddressBase
{

 

    public Guid Id { get; set; }
    public string UserId { get; set; }=string.Empty;
    public ApplicationUser? User { get; set; }
}