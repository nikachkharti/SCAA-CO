using SCAA_API.Data;
using SCAA_API.Entities;
using SCAA_API.Repository.Base;
using SCAA_API.Repository.Contracts;

namespace SCAA_API.Repository
{
    public class CategoryRepository : RepositoryBase<Category, ApplicationDbContext>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
