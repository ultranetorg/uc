using ColorArray = Microsoft.Maui.Graphics.Colors;

namespace UC.Net.Node.MAUI.ViewModels
{
    public partial class CreateAccountViewModel : BaseViewModel
    {
		public Page Page { get; }
       
		[ObservableProperty]
        CustomCollection<AccountColor> _colors = new();
		
		[ObservableProperty]
        AccountColor _selectedAccountColor;

        public CreateAccountViewModel(ILogger<CreateAccountViewModel> logger) : base(logger)
        {
            Page = (Application.Current.MainPage as NavigationPage).RootPage;
			AddFakeData();
            SelectedAccountColor = Colors.First();
        }

		[RelayCommand]
        private void Randomize()
        {
            ColorTapped(Colors[new Random().Next(0, Colors.Count)]);
        }
		
		[RelayCommand]
        private void ColorTapped(AccountColor obj)
        {
            foreach (var item in Colors)
            {
                item.BoderColor = ColorArray.Transparent;
                Colors.ReportItemChange(item);
            }
            obj.BoderColor = Page.BackgroundColor;
            Colors.ReportItemChange(obj);
            SelectedAccountColor = obj;
        }

		private void AddFakeData()
		{
			// TODO: refactoring, get background color from resources
			Colors.Add(new AccountColor { Color = Color.FromArgb("#6601e3"), BoderColor = Page.BackgroundColor });
			Colors.Add(new AccountColor { Color = Color.FromArgb("#3765f4"), BoderColor = ColorArray.Transparent });
			Colors.Add(new AccountColor { Color = Color.FromArgb("#4cb16c"), BoderColor = ColorArray.Transparent });
			Colors.Add(new AccountColor { Color = Color.FromArgb("#ba918c"), BoderColor = ColorArray.Transparent });
			Colors.Add(new AccountColor { Color = Color.FromArgb("#d56a48"), BoderColor = ColorArray.Transparent });
			Colors.Add(new AccountColor { Color = Color.FromArgb("#56d7de"), BoderColor = ColorArray.Transparent });
			Colors.Add(new AccountColor { Color = Color.FromArgb("#bb50dd"), BoderColor = ColorArray.Transparent });
		}
    }
}
