using SCAA_API.Models.Category;

namespace SCAA_API.Services.Contracts
{
    public interface ICategoryService
    {
        Task<List<CategoryForGettingDto>> GetAllCategoriesAsync();
        Task<CategoryForGettingDto> GetCategoryWithIdAsync(int categoryId);
        Task<int> AddNewCategoryAsync(CategoryForCreatingDto model);
        Task<int> DeleteCategoryAsync(int categoryId);
        Task<int> UpdateCategoryAsync(CategoryForUpdatingDto model);
    }
}
