using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCAA_API.Models.Common;
using SCAA_API.Models.Supplier;
using SCAA_API.Services.Contracts;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace SCAA_API.Controllers
{
    [Route("api/suppliers")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class SuppliersController(ISupplierService supplierService) : ControllerBase
    {
        /// <summary>
        /// List All Suppliers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSuppliers([FromQuery] PagedRequestDto parameters)
        {
            var result = await supplierService.GetAllSuppliersAsync(parameters);

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
        /// Get Supplier with Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var result = await supplierService.GetSupplierWithIdAsync(id);

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
        /// Create a new Supplier
        /// </summary>
        [HttpPost]
        [SwaggerRequestExample(typeof(SupplierForCreatingDto), typeof(SupplierForCreatingDtoExample))]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierForCreatingDto model)
        {
            var result = await supplierService.AddNewSupplierAsync(model);
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
        /// Delete a Supplier
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            await supplierService.DeleteSupplierAsync(id);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK)
            };
            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// Update a Supplier
        /// </summary>
        [HttpPut]
        [SwaggerRequestExample(typeof(SupplierForUpdatingDto), typeof(SupplierForUpdatingDtoExample))]
        public async Task<IActionResult> UpdateSupplier([FromBody] SupplierForUpdatingDto model)
        {
            var result = await supplierService.UpdateSupplierAsync(model);

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
