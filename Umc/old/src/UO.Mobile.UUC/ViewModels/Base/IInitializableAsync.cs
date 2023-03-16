namespace UO.Mobile.UUC.ViewModels.Base;

public interface IInitializableAsync
{
    bool IsInitialized { get; protected set; }
    bool MultipleInitialization => false;

    Task InitializeAsync(object parameter);
}
