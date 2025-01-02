using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Uccs.Web.Configurations;

public static class ConfigureCorsPolicy
{
	private const string AllowAnyOrigin = "*";

	public static IServiceCollection AddCorsPolicy(this IServiceCollection services, ConfigurationManager configurationManager)
	{
		Guard.Against.Null(services);
		Guard.Against.Null(configurationManager);

		var allowedOrigins = configurationManager.Get<AllowedOriginsConfiguration>();
		if (allowedOrigins != null && allowedOrigins.AllowedOrigins?.Length > 0)
		{
			services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					if (allowedOrigins.AllowedOrigins.Contains(AllowAnyOrigin))
					{
						policy.AllowAnyOrigin();
					}
					else
					{
						policy.WithOrigins(allowedOrigins.AllowedOrigins);
					}
				});
			});
		}

		return services;
	}
}
