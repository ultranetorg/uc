using CreateAccountViewModel = UC.Umc.ViewModels.Views.CreateAccountViewModel;
namespace UC.Umc.Views;

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