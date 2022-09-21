using CreateAccountViewModel = UC.Net.Node.MAUI.ViewModels.Views.CreateAccountViewModel;
namespace UC.Net.Node.MAUI.Views;

public partial class CreateAccount2View : ContentView
{
    public CreateAccount2View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreateAccountViewModel>();
    }

    public CreateAccount2View(CreateAccountViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}