﻿namespace UC.Umc.Pages;

public partial class DeleteAccountPage : CustomPage
{
  //  public DeleteAccountPage()
  //  {
  //      InitializeComponent();
  //      var vm = Ioc.Default.GetService<DeleteAccountViewModel>();
		//vm.Initialize(DefaultDataMock.Account1);
		//BindingContext = vm;
  //  }

	public DeleteAccountPage(AccountViewModel account)
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<DeleteAccountViewModel>();
		vm.Initialize(account);
        BindingContext = vm;
    }

	public DeleteAccountPage(AccountViewModel account, DeleteAccountViewModel vm)
    {
        InitializeComponent();
		vm.Initialize(account);
        BindingContext = vm;
    }
}