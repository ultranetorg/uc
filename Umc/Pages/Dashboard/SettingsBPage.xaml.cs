﻿namespace UC.Umc.Pages;

public partial class SettingsBPage : CustomPage
{
    public SettingsBPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<SettingsBViewModel>();
    }

    public SettingsBPage(SettingsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
