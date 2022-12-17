using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Stripe;

namespace Fresh724.Entity.Entities;

public class CompanyRating
{
    [Key]
    public Guid id { get; set; }
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }
    
    [DefaultValue("Anonymous")]
    public string UserFullName { get; set; }=string.Empty;
        
    public Guid CompanyId { get; set; }
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; }
    
    public Guid OrderId { get; set; }
    [ForeignKey("OrderId")]
    public virtual OrderShopping Order { get; set; }
    public DateTime CreateDate { get; set; }=DateTime.Now;
    
    public string Comment { get; set; }=string.Empty;
    
  
    public int Rating { get; set; }
     
    
}