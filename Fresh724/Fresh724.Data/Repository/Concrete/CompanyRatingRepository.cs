using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;

namespace Fresh724.Data.Repository.Concrete;

public class CompanyRatingRepository : EntityRepository<CompanyRating>, ICompanyRatingRepository
{
    private ApplicationDbContext _db;

    public CompanyRatingRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
}