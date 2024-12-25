using AutoMapper.Internal;
using Explorer.WebApi;

namespace Explorer.Api.Configurations;

public static class AutoMapperExtensions
{
	public static IServiceCollection AddAutoMapper(this IServiceCollection services)
	{
		Guard.Against.Null(services);

		services.AddAutoMapper(config =>
		{
			config.AllowNullCollections = true;

			ScanAssembliesForMaps(config);

			// NOTE: should be removed after AutoMapper upgrading to 12.0.0 version.
			config.Internal().MethodMappingEnabled = false;
		});

		return services;
	}

	private static void ScanAssembliesForMaps(IMapperConfigurationExpression config)
	{
		config.AddMaps(typeof(_).Assembly);
		config.AddMaps(typeof(BLL._).Assembly);
	}
}
