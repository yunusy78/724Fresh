using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fresh724.Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Fresh724.Entity.Entities;

public class OrderShoppingDetails
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
  
    public Product Product { get; set; }

    public int Quantity { get; set; }
    
    public double Price { get; set; }
    
    
    [ForeignKey("OrderId")]
    public Guid OrderId {  get; set; }
    [ValidateNever]
    public OrderShopping Order { get; set; }
    
}