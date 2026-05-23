using MapsterMapper;
using SCAA_API.Entities;
using SCAA_API.Models.Category;
using SCAA_API.Repository.Contracts;
using SCAA_API.Services.Contracts;

namespace SCAA_API.Services
{
    public class CategoryService(ICategoryRepository categoryRepository, IMapper mapper) : ICategoryService
    {
        public async Task<int> AddNewCategoryAsync(CategoryForCreatingDto model)
        {
            var category = mapper.Map<Category>(model);
            await categoryRepository.AddAsync(category);
            await categoryRepository.SaveAsync();
            return category.Id;
        }

        public async Task<int> DeleteCategoryAsync(int categoryId)
        {
            var category = await categoryRepository.GetAsync(x => x.Id == categoryId);

            if (category == null)
                throw new Exception("Category not found");

            categoryRepository.Remove(category);
            await categoryRepository.SaveAsync();
            return category.Id;
        }

        public async Task<List<CategoryForGettingDto>> GetAllCategoriesAsync()
        {
            var categories = await categoryRepository.GetAllAsync();
            return mapper.Map<List<CategoryForGettingDto>>(categories.Items);
        }

        public async Task<CategoryForGettingDto> GetCategoryWithIdAsync(int categoryId)
        {
            var category = await categoryRepository.GetAsync(x => x.Id == categoryId);
            return mapper.Map<CategoryForGettingDto>(category);
        }

        public async Task<int> UpdateCategoryAsync(CategoryForUpdatingDto model)
        {
            var category = await categoryRepository.GetAsync(x => x.Id == model.Id);

            if (category == null)
                throw new Exception("Category not found");

            mapper.Map(model, category);
            categoryRepository.Update(category);
            await categoryRepository.SaveAsync();

            return category.Id;
        }
    }
}
