using SCAA_API.Models.Authentication;
using SCAA_API.Models.Category;
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
