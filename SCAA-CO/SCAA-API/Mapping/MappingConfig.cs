using Mapster;
using SCAA_API.Entities;
using SCAA_API.Entities.Authentication;
using SCAA_API.Models.Authentication;
using SCAA_API.Models.Category;

namespace SCAA_API.Mapping
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RegistrationRequestDto, ApplicationUser>()
                .Map(dest => dest.UserName, src => src.Email)
                .Map(dest => dest.NormalizedUserName, src => src.Email.ToUpper())
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.NormalizedEmail, src => src.Email.ToUpper());

            config.NewConfig<Category, CategoryForGettingDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CategoryName, src => src.CategoryName);

            config.NewConfig<CategoryForCreatingDto, Category>()
                .Map(dest => dest.CategoryName, src => src.CategoryName);

            config.NewConfig<CategoryForUpdatingDto, Category>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CategoryName, src => src.CategoryName);
        }
    }
}
