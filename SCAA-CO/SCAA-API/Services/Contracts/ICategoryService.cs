using SCAA_API.Models.Category;
using SCAA_API.Models.Common;

namespace SCAA_API.Services.Contracts
{
    public interface ICategoryService
    {
        Task<PagedResponseDto<CategoryForGettingDto>> GetAllCategoriesAsync(PagedRequestDto parameters);
        Task<CategoryForGettingDto> GetCategoryWithIdAsync(int categoryId);
        Task<int> AddNewCategoryAsync(CategoryForCreatingDto model);
        Task<int> DeleteCategoryAsync(int categoryId);
        Task<int> UpdateCategoryAsync(CategoryForUpdatingDto model);
    }
}
