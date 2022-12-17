using System.ComponentModel.DataAnnotations;

namespace Fresh724.Core.Entities;

public class AddressBase:IAddressBase
{
    
    public string Street1 { get; set; }=string.Empty;
    public string Street2 { get; set; }=string.Empty;
    public string City { get; set; }=string.Empty;
    public string State { get; set; }=string.Empty;
    
    public int ZipCode { get; set; }
    public string Country { get; set; }=string.Empty;
}

                                                                                                                                                         