using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCAA_API.Models.Category;
using SCAA_API.Models.Common;
using SCAA_API.Services.Contracts;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace SCAA_API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class CategoriesController(ICategoryService categoryService) : ControllerBase
    {
        /// <summary>
        /// List All Categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories(
            [FromQuery] PagedRequestDto parameters)
        {
            var result = await categoryService.GetAllCategoriesAsync(parameters);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = result
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// Get Category with Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var result = await categoryService.GetCategoryWithIdAsync(id);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = result
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// Create a new Category
        /// </summary>
        [HttpPost]
        [SwaggerRequestExample(typeof(CategoryForCreatingDto), typeof(CategoryForCreatingDtoExample))]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryForCreatingDto model)
        {
            var result = await categoryService.AddNewCategoryAsync(model);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.Created),
                Result = result
            };
            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// Delete a Category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await categoryService.DeleteCategoryAsync(id);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK)
            };
            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// Update a Category
        /// </summary>
        [HttpPut]
        [SwaggerRequestExample(typeof(CategoryForUpdatingDto), typeof(CategoryForUpdatingDtoExample))]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryForUpdatingDto model)
        {
            var result = await categoryService.UpdateCategoryAsync(model);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = result
            };
            return StatusCode(response.HttpStatusCode, response);
        }

    }
}