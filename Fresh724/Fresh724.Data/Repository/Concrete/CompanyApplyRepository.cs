using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;

namespace Fresh724.Data.Repository.Concrete;

public class CompanyApplyRepository : EntityRepository<CompanyApply>, ICompanyApplyRepository
{
    private ApplicationDbContext _db;

    public CompanyApplyRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

}