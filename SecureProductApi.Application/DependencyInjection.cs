using Microsoft.Extensions.DependencyInjection;

namespace SecureProductApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Later: AddValidatorsFromAssembly(...) if you add FluentValidation, AddAutoMapper, etc.
            return services;
        }
    }
}