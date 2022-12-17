using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fresh724.Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Fresh724.Entity.Entities;

public class OrderShopping:AddressBase
{
    public Guid Id { get; set; }
    [Required]
    [Display(Name = "First Name")]
    [Column(TypeName = "nvarchar(15)")]
    public string FirstName { get; set; }=string.Empty;
    
    
    
    [Required]
    [Display(Name = "Last Name")]
    [Column(TypeName = "nvarchar(15)")]
    public string LastName { get; set; }=string.Empty;
    
    
    public string Email { get; set; }=string.Empty;
    
    
    public DateTime OrderDate { get; set; }
    
    
    public int Quantity { get; set; }
    
    public double Price { get; set; }
    
    public double TotalPrice { get; set; }

    public string OrderStatus { get; set; }=string.Empty;
    
    public string PaymentStatus { get; set; }=string.Empty;
    
    public string PaymentType { get; set; }=string.Empty;
    
    public DateTime PaymentDate { get; set; }
    public DateTime PaymentDueDate { get; set; }

    public string SessionId { get; set; }=string.Empty;
    public string PaymentIntentId { get; set; }=string.Empty;
    
    
    [StringLength(11,MinimumLength = 11, ErrorMessage = "PhoneNumber must be 11 characters")]
    public string PhoneNumber { get; set; }
    
    [ForeignKey("UserId")]
    public string UserId {  get; set; }=string.Empty;
    [ValidateNever]
    public ApplicationUser User { get; set; }
    
    [NotMapped]
    public Status ShoppingStatus { get; set; }
    
    public enum Status
    {
        Pending, Approved, Rejected,Processing,ReadyForPickup,Completed,Cancelled,Active,Unavailable
    }
   
    
}
