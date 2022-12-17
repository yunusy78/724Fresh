using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;

namespace Fresh724.Data.Repository.Concrete;

public class CartRepository : EntityRepository<Cart>, ICartRepository
{
    private ApplicationDbContext _db;

    public CartRepository(ApplicationDbContext context) : base(context)
    {
        _db = context;
    }

    public int IncrementCount(Cart shoppingCart, int count)
    {
        shoppingCart.Quantity += count;
        return shoppingCart.Quantity;
    }

    public int DecrementCount(Cart shoppingCart, int count)
    {
        shoppingCart.Quantity -= count;
        return shoppingCart.Quantity;
    }
}  
