namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class CreateAccountViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    AccountColor _selectedAccountColor;

    public CreateAccountViewModel(ILogger<CreateAccountViewModel> logger) : base(logger)
    {
		AddFakeData();
        SelectedAccountColor = ColorsCollection.First();
    }

	[RelayCommand]
    private void Randomize()
    {
        ColorTappedCommand.Execute(ColorsCollection[new Random().Next(0, ColorsCollection.Count)]);
    }

	private void AddFakeData()
	{
		// TODO: refactoring, get background color from resources
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#6601e3"), BoderColor = Shell.Current.BackgroundColor });
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#3765f4"), BoderColor = Colors.Transparent });
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#4cb16c"), BoderColor = Colors.Transparent });
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#ba918c"), BoderColor = Colors.Transparent });
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#d56a48"), BoderColor = Colors.Transparent });
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#56d7de"), BoderColor = Colors.Transparent });
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#bb50dd"), BoderColor = Colors.Transparent });
	}
}
