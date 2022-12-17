using System.Linq.Expressions;
using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;

namespace Fresh724.Data.Repository.Concrete;

public class OrderShoppingRepository:EntityRepository<OrderShopping>,IOrderShoppingRepository
{
    private ApplicationDbContext _db;
    public OrderShoppingRepository(ApplicationDbContext context) : base(context)
    {
        _db = context;
    }
    public void Update(OrderShopping obj)
    {
        _db.Orders.Update(obj);
    }
    
    public void UpdateStatus(Guid id, string orderStatus, string? paymentStatus = null)
    {
        var orderFromDb = _db.Orders.FirstOrDefault(u => u.Id == id);
        if (orderFromDb != null)
        {
            orderFromDb.OrderStatus = orderStatus;
            if (paymentStatus != null)
            {
                orderFromDb.PaymentStatus = paymentStatus;
            }
        }
    }

    public void UpdateStripePaymentID(Guid id, string sessionId, string paymentIntentId)
    {
        var orderFromDb = _db.Orders.FirstOrDefault(u => u.Id == id);
        orderFromDb.PaymentDate = DateTime.Now;
        orderFromDb.SessionId = sessionId;
        orderFromDb.PaymentIntentId = paymentIntentId;
            
    }
    public void UpdateStripeSessionId(Guid id, string sessionId)
    {
        var order = _db.Orders.FirstOrDefault(u => u.Id == id);
        order.PaymentDate = DateTime.Now;
        order.SessionId = sessionId;

    }
}
