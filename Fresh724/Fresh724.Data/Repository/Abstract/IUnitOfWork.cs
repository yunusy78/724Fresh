using Fresh724.Data.Repository.Concrete;

namespace Fresh724.Data.Repository.Abstract;

public interface IUnitOfWork

{
 IApplicationUserRepository ApplicationUsers { get; }
  ICategoryRepository Categories { get; }
  IProductRepository Products { get; }
  IOrderShoppingRepository Orders { get; }
  IEmployeeRepository Employees { get; }
  ICompanyRepository Companies { get; }
  ICartRepository Carts { get; }
  
  ICompanyApplyRepository CompanyApplies { get; }
  
   IOrderShoppingDetailsRepository OrderDetails { get; }
  
  AddressUserRepository AddressUsers { get; }
  
  ICompanyRatingRepository CompanyRatings { get; }
  void SaveChanges();
 
    
    
}