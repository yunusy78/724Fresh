using Fresh724.Entity.Entities;

namespace Fresh724.Data.Repository.Abstract;

public interface ICartRepository: IEntityRepository<Cart>

{
    int IncrementCount(Cart shoppingCart, int count);
    int DecrementCount(Cart shoppingCart, int count);
}