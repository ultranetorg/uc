namespace UC.Net.Node.MAUI.ViewModels;
public static class ViewModelExtensions
{
    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
    {
        // Transient ViewModels
        // builder.Services.AddTransient(sp => new CreateAccountViewModel(sp.GetService<ILogger<CreateAccountViewModel>>()));

        // Singleton ViewModels

        return builder;
    }
}
