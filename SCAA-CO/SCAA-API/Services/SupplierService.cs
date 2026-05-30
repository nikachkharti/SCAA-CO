using MapsterMapper;
using SCAA_API.Entities;
using SCAA_API.Exceptions;
using SCAA_API.Models.Common;
using SCAA_API.Models.Supplier;
using SCAA_API.Repository.Contracts;
using SCAA_API.Services.Contracts;
using System.Linq.Expressions;

namespace SCAA_API.Services
{
    public class SupplierService(ISupplierRepository supplierRepository, IMapper mapper) : ISupplierService
    {
        public async Task<int> AddNewSupplierAsync(SupplierForCreatingDto model)
        {
            var supplier = mapper.Map<Supplier>(model);
            await supplierRepository.AddAsync(supplier);
            await supplierRepository.SaveAsync();
            return supplier.Id;
        }

        public async Task<int> DeleteSupplierAsync(int supplierId)
        {
            if (supplierId <= 0)
                throw new BadRequestException("Supplier Id can't be a negative number");

            var supplier = await supplierRepository.GetAsync(x => x.Id == supplierId);

            if (supplier == null)
                throw new NotFoundException("Supplier not found");

            supplierRepository.Remove(supplier);
            await supplierRepository.SaveAsync();
            return supplier.Id;
        }

        public async Task<PagedResponseDto<SupplierForGettingDto>> GetAllSuppliersAsync(PagedRequestDto parameters)
        {
            Expression<Func<Supplier, object>> orderBy = parameters.SortBy?.ToLower() switch
            {
                "id" => x => x.Id,
                "supplierName" => x => x.SupplierName,
                _ => x => x.Id
            };

            var suppliers = await supplierRepository.GetAllAsync(
                pageNumber: parameters.PageNumber,
                pageSize: parameters.PageSize,
                orderBy: orderBy,
                ascending: parameters.Ascending
            );

            if (!suppliers.Items.Any())
                throw new NotFoundException("Suppliers not found");

            return new PagedResponseDto<SupplierForGettingDto>
            {
                Items = mapper.Map<List<SupplierForGettingDto>>(suppliers.Items),
                TotalCount = suppliers.TotalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<SupplierForGettingDto> GetSupplierWithIdAsync(int supplierId)
        {
            if (supplierId <= 0)
                throw new BadRequestException("Supplier Id can't be a negative number");

            var supplier = await supplierRepository.GetAsync(x => x.Id == supplierId);

            if (supplier == null)
                throw new NotFoundException("Supplier not found");

            return mapper.Map<SupplierForGettingDto>(supplier);
        }

        public async Task<int> UpdateSupplierAsync(SupplierForUpdatingDto model)
        {
            if (model.Id <= 0)
                throw new BadRequestException("Supplier Id can't be a negative number");

            var supplier = await supplierRepository.GetAsync(x => x.Id == model.Id);

            if (supplier == null)
                throw new NotFoundException("Supplier not found");

            mapper.Map(model, supplier);
            supplierRepository.Update(supplier);
            await supplierRepository.SaveAsync();

            return supplier.Id;
        }
    }
}
