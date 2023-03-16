namespace UO.Mobile.UUC.ViewModels.Base;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private bool _isBuisy;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool IsBuisy
    {
        get => _isBuisy;
        set
        {
            _isBuisy = value;
            OnPropertyChanged(nameof(IsBuisy));
        }
    }
}
