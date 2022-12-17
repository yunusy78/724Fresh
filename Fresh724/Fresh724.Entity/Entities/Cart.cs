using System.ComponentModel.DataAnnotations.Schema;

namespace Fresh724.Entity.Entities;
/*public class ShoppingCartList
{
        private List<Cart> _shoppingCart = new List<Cart>();
        public List<Cart> ShoppingCart
        {
            get { return _shoppingCart; }
            set { _shoppingCart = value; }
        }
        
        public void AddItem(Product product, int quantity)
        {
            Cart item = _shoppingCart.FirstOrDefault(p => p.Product.Id == product.Id);
            if (item == null)
            {
                _shoppingCart.Add(new Cart { Product = product, Quantity = quantity });
            }
            else
            {
                item.Quantity += quantity;
            }
        }
        
        public void DeleteItem(Product product)
        {
            _shoppingCart.RemoveAll(p => p.Product.Id == product.Id);
        }
        
        public double TotalPrice()
        {
            return _shoppingCart.Sum(p => p.Product.PurchasePrice * p.Quantity);
        }
        public void clearCart()
        {
            _shoppingCart.Clear();
        }
}*/
public class Cart
{
    
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]

    public Product Product { get; set; }
    public int Quantity { get; set; }

    [ForeignKey("UserId")]
    public string UserId { get; set; }
    

    public ApplicationUser User { get; set; }
    
    
    [NotMapped]
    public double Price { get; set; }
}
