using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Fresh724.Entity.Entities;

public class ApplicationUser:IdentityUser
{
    
    [Display(Name = "CompanyId")]
    public Guid CompanyId{ get; set; }
    
  
    [MaybeNull]
    [Display(Name = "FirstName")]
    public string FirstName{ get; set; }=string.Empty;
    
    [MaybeNull]
    [Display(Name = "LastName")]
    public string LastName{ get; set; }=string.Empty;
}