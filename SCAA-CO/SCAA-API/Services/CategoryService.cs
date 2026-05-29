using MapsterMapper;
using SCAA_API.Entities;
using SCAA_API.Exceptions;
using SCAA_API.Models.Category;
using SCAA_API.Models.Common;
using SCAA_API.Repository.Contracts;
using SCAA_API.Services.Contracts;
using System.Linq.Expressions;

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
            if (categoryId <= 0)
                throw new BadRequestException("Category Id can't be a negative number");

            var category = await categoryRepository.GetAsync(x => x.Id == categoryId);

            if (category == null)
                throw new NotFoundException("Category not found");

            categoryRepository.Remove(category);
            await categoryRepository.SaveAsync();
            return category.Id;
        }

        public async Task<PagedResponseDto<CategoryForGettingDto>> GetAllCategoriesAsync(PagedRequestDto parameters)
        {
            Expression<Func<Category, object>> orderBy = parameters.SortBy?.ToLower() switch
            {
                "id" => x => x.Id,
                "categoryName" => x => x.CategoryName,
                _ => x => x.Id
            };

            var categories = await categoryRepository.GetAllAsync(
                pageNumber: parameters.PageNumber,
                pageSize: parameters.PageSize,
                orderBy: orderBy,
                ascending: parameters.Ascending
            );

            if (!categories.Items.Any())
                throw new NotFoundException("Categories not found");

            return new PagedResponseDto<CategoryForGettingDto>
            {
                Items = mapper.Map<List<CategoryForGettingDto>>(categories.Items),
                TotalCount = categories.TotalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public async Task<CategoryForGettingDto> GetCategoryWithIdAsync(int categoryId)
        {
            if (categoryId <= 0)
                throw new BadRequestException("Category Id can't be a negative number");

            var category = await categoryRepository.GetAsync(x => x.Id == categoryId);

            if (category == null)
                throw new NotFoundException("Category not found");

            return mapper.Map<CategoryForGettingDto>(category);
        }

        public async Task<int> UpdateCategoryAsync(CategoryForUpdatingDto model)
        {
            if (model.Id <= 0)
                throw new BadRequestException("Category Id can't be a negative number");

            var category = await categoryRepository.GetAsync(x => x.Id == model.Id);

            if (category == null)
                throw new NotFoundException("Category not found");

            mapper.Map(model, category);
            categoryRepository.Update(category);
            await categoryRepository.SaveAsync();

            return category.Id;
        }
    }
}
