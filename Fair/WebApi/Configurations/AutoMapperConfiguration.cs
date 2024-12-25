namespace Explorer.Api.Configurations;

#if DEBUG
public static class AutoMapperConfiguration
{
	public static void AssertAutoMapperConfiguration(this IServiceProvider serviceProvider)
	{
		Guard.Against.Null(serviceProvider);

		IMapper mapper = serviceProvider.GetRequiredService<IMapper>();
		mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}
}

#endif
