using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Fresh724.Entity.Entities;

public class CartList
{
    
    public IEnumerable<Cart> CartItems { get; set; }
    
    public OrderShopping OrderShopping { get; set; }
}