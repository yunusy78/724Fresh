using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fresh724.Entity.Entities;

public class OrderShoppingList
{
    [ValidateNever]
    public OrderShopping OrderShopping { get; set; }
    
    [ValidateNever]
    public IEnumerable<OrderShoppingDetails> OrderShoppingDetails { get; set; }
    
}