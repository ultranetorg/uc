namespace UC.Umc.Views;

public partial class CreateAccount2View : ContentView
{
    public CreateAccount2View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreateAccount2ViewModel>();
    }

    public CreateAccount2View(CreateAccount2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}