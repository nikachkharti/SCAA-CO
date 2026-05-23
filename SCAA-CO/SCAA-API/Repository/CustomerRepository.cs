using SCAA_API.Data;
using SCAA_API.Entities;
using SCAA_API.Repository.Base;
using SCAA_API.Repository.Contracts;

namespace SCAA_API.Repository
{
    public class CustomerRepository : RepositoryBase<Customer, ApplicationDbContext>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
