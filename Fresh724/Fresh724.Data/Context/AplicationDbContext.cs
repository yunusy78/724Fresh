using System.Reflection;
using Fresh724.Entity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Fresh724.Data.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    
    
    public DbSet<Category> Categories => Set<Category>();
    
    public DbSet<AddressUser> Addresses => Set<AddressUser>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Product> Products => Set<Product>();
    
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Cart> ShoppingCarts => Set<Cart>();
    
    public DbSet<OrderShopping> Orders => Set<OrderShopping>();
    public DbSet<OrderShoppingDetails> OrderDetails => Set<OrderShoppingDetails>();
    public DbSet<CompanyApply> CompanyApplies => Set<CompanyApply>();
    public DbSet<CompanyRating> CompanyRatings => Set<CompanyRating>();



}