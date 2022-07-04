using UC.Net.Node.MAUI.Controls;
using UC.Net.Node.MAUI.Models;
using UC.Net.Node.MAUI.Popups;
using UC.Net.Node.MAUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UC.Net.Node.MAUI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthorRegistrationRenewalStepTwoView : ContentView
    {
        AuthorRegistrationRenewalStepTwoViewModel _viewModel;
        public AuthorRegistrationRenewalStepTwoView()
        {
            InitializeComponent();
            BindingContext= _viewModel= new AuthorRegistrationRenewalStepTwoViewModel();
        }
    }
    public class AuthorRegistrationRenewalStepTwoViewModel : BaseViewModel
    {
        public AuthorRegistrationRenewalStepTwoViewModel()
        {
            

        }
      
       
    }
}