namespace UC.Net.Node.MAUI.ViewModels;

public static class ViewModelExtensions
{
    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
    {
        // Transient ViewModels
        builder.Services.AddTransient(sp => new AboutViewModel(
			App.ServiceProvider.GetService<ILogger<AboutViewModel>>()));
        builder.Services.AddTransient(sp => new HelpViewModel(
			App.ServiceProvider.GetService<ILogger<HelpViewModel>>()));
        builder.Services.AddTransient(sp => new HelpBViewModel(
			App.ServiceProvider.GetService<ILogger<HelpBViewModel>>()));
        builder.Services.AddTransient(sp => new WhatsNewViewModel(
			App.ServiceProvider.GetService<ILogger<WhatsNewViewModel>>()));

        // Singleton ViewModels

        return builder;
    }
}
