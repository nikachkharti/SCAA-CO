using SCAA_API.Data;
using SCAA_API.Entities;
using SCAA_API.Repository.Base;

namespace SCAA_API.Repository.Contracts
{
    public interface ISupplierRepository : IRepositoryBase<Supplier, ApplicationDbContext>
    {
    }
}
