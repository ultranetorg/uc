namespace UC.Net.Node.MAUI.Services;

public static class ServiceExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        // Core Services
        builder.Services.AddSingleton<IServicesMockData, ServicesMockData>(sp => new ServicesMockData());

		// TODO: add other services

        return builder;
    }
}
