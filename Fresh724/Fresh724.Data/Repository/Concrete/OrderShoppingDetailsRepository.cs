using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;

namespace Fresh724.Data.Repository.Concrete;

public class OrderShoppingDetailsRepository : EntityRepository<OrderShoppingDetails>, IOrderShoppingDetailsRepository
{
    private ApplicationDbContext _db;

    public OrderShoppingDetailsRepository(ApplicationDbContext context) : base(context)
    {
        _db = context;
    }
}




    
