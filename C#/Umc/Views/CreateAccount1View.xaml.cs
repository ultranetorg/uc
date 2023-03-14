namespace UC.Umc.Views;

public partial class CreateAccount1View : ContentView
{
    public CreateAccount1View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreateAccount1ViewModel>();
    }

    public CreateAccount1View(CreateAccount1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}