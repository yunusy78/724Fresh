using Fresh724.Entity.Entities;

namespace Fresh724.Data.Repository.Abstract;

public interface IOrderShoppingRepository: IEntityRepository<OrderShopping>

{
    void Update(OrderShopping obj);
    void UpdateStatus(Guid id, string orderStatus, string? paymentStatus=null);
    public void UpdateStripePaymentID(Guid id, string sessionId, string paymentIntentId);
    public void UpdateStripeSessionId(Guid id, string sessionId);
}