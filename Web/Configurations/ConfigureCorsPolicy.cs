using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Uccs.Web.Configurations;

public static class ConfigureCorsPolicy
{
	private const string AllowAnyOrigin = "*";

	public static IServiceCollection AddCorsPolicy(this IServiceCollection services, ConfigurationManager configurationManager)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configurationManager);

		var allowedOrigins = configurationManager.Get<AllowedOriginsConfiguration>();
		if(allowedOrigins != null && allowedOrigins.AllowedOrigins?.Length > 0)
		{
			services.AddCorsPolicy(allowedOrigins.AllowedOrigins);
		}

		return services;
	}

	public static IServiceCollection AddCorsPolicy(this IServiceCollection services, [NotEmpty] string[] allowedOrigins)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(allowedOrigins);

		if(allowedOrigins.Length == 0)
			throw new ArgumentException();

		if(allowedOrigins != null)
		{
			services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					if(allowedOrigins.Contains(AllowAnyOrigin))
					{
						policy.AllowAnyOrigin();
					}
					else
					{
						policy.WithOrigins(allowedOrigins);
					}
				});
			});
		}

		return services;
	}
}
