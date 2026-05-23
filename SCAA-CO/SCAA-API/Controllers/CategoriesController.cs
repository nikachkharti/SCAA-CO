using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SCAA_API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        /// <summary>
        /// List All Categories
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult GetCategories()
        {
            var categories = new List<string> { "Category 1", "Category 2", "Category 3" };
            return Ok(categories);
        }
    }
}
