using SCAA_API.Models.Common;
using SCAA_API.Models.Supplier;

namespace SCAA_API.Services.Contracts
{
    public interface ISupplierService
    {
        Task<PagedResponseDto<SupplierForGettingDto>> GetAllSuppliersAsync(PagedRequestDto parameters);
        Task<SupplierForGettingDto> GetSupplierWithIdAsync(int supplierId);
        Task<int> AddNewSupplierAsync(SupplierForCreatingDto model);
        Task<int> DeleteSupplierAsync(int supplierId);
        Task<int> UpdateSupplierAsync(SupplierForUpdatingDto model);
    }
}
