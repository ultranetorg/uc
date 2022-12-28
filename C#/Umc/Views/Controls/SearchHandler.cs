using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace UC.Umc.Controls;

public class AuthorSearchHandler : SearchHandler
{
    public Type SelectedItemNavigationTarget { get; set; }

    protected override void OnQueryChanged(string oldValue, string newValue)
    {
        base.OnQueryChanged(oldValue, newValue);

        // Check oldValue for null to prevent sending request on page's first load
        if (oldValue != null && newValue.Length is >= 3 or 0)
        {
            WeakReferenceMessenger.Default.Send(new AuthorsSearchMessage(newValue));
        }
    }

    protected override async void OnItemSelected(object item)
    {
        base.OnItemSelected(item);

        // Let the animation complete
        await Task.Delay(1000);

        ShellNavigationState state = (App.Current.MainPage as Shell).CurrentState;
        // The following route works because route names are unique in this app.
        //await Shell.Current.GoToAsync(GetNavigationTarget());
    }
}

public class AuthorsSearchMessage : ValueChangedMessage<string>
{
    public AuthorsSearchMessage(string value) : base(value)
    {
    }
}
