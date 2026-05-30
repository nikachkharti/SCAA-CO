using SCAA_API.Models.Authentication;
using SCAA_API.Models.Category;
using SCAA_API.Models.Supplier;
using Swashbuckle.AspNetCore.Filters;

namespace SCAA_API;

public sealed record RegistrationRequestDtoExample : IExamplesProvider<RegistrationRequestDto>
{
    public RegistrationRequestDto GetExamples() =>
        new RegistrationRequestDto() { Email = "admin@gmail.com", Password = "Password123!" };
}

public sealed record LoginRequestDtoExample : IExamplesProvider<LoginRequestDto>
{
    public LoginRequestDto GetExamples() =>
        new LoginRequestDto() { UserName = "admin@gmail.com", Password = "Password123!" };
}


public sealed record CategoryForCreatingDtoExample : IExamplesProvider<CategoryForCreatingDto>
{
    public CategoryForCreatingDto GetExamples() =>
        new CategoryForCreatingDto() { CategoryName = "Test Category" };
}

public sealed record CategoryForUpdatingDtoExample : IExamplesProvider<CategoryForUpdatingDto>
{
    public CategoryForUpdatingDto GetExamples() =>
        new CategoryForUpdatingDto() { Id = 1, CategoryName = "Updated Test Category" };
}


public sealed record SupplierForCreatingDtoExample : IExamplesProvider<SupplierForCreatingDto>
{
    public SupplierForCreatingDto GetExamples() =>
        new SupplierForCreatingDto() { SupplierName = "Test Supplier" };
}

public sealed record SupplierForUpdatingDtoExample : IExamplesProvider<SupplierForUpdatingDto>
{
    public SupplierForUpdatingDto GetExamples() =>
        new SupplierForUpdatingDto() { Id = 1, SupplierName = "Updated Test Supplier" };
}
