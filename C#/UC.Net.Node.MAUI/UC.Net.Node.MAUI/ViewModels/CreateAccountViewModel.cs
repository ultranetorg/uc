
using UC.Net.Node.MAUI.Controls;
using ColorArray = Microsoft.Maui.Graphics.Colors;

namespace UC.Net.Node.MAUI.ViewModels
{
    public partial class CreateAccountViewModel : BaseViewModel
    {
        Page Page;
        public CreateAccountViewModel(ILogger<CreateAccountViewModel> logger):base(logger)
        {
			// TODO: refactoring, get background color from resources
            Page = (Application.Current.MainPage as NavigationPage).RootPage;
            Colors.Add(new AccountColor { Color = Color.FromArgb("#6601e3"), BoderColor = Page.BackgroundColor });
            Colors.Add(new AccountColor { Color = Color.FromArgb("#3765f4"), BoderColor = ColorArray.Transparent });
            Colors.Add(new AccountColor { Color = Color.FromArgb("#4cb16c"), BoderColor = ColorArray.Transparent });
            Colors.Add(new AccountColor { Color = Color.FromArgb("#ba918c"), BoderColor = ColorArray.Transparent });
            Colors.Add(new AccountColor { Color = Color.FromArgb("#d56a48"), BoderColor = ColorArray.Transparent });
            Colors.Add(new AccountColor { Color = Color.FromArgb("#56d7de"), BoderColor = ColorArray.Transparent });
            Colors.Add(new AccountColor { Color = Color.FromArgb("#bb50dd"), BoderColor = ColorArray.Transparent });

            SelectedAccountColor= Colors.First();
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
       
        CustomCollection<AccountColor> _colors = new CustomCollection<AccountColor>();
        public CustomCollection<AccountColor> Colors
        {
            get { return _colors; }
            set { SetProperty(ref _colors, value); }
        }

        AccountColor _selectedAccountColor;
        public AccountColor SelectedAccountColor
        {
            get { return _selectedAccountColor; }
            set { SetProperty(ref _selectedAccountColor, value); }
        }
    }
}
