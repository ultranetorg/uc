﻿namespace UC.Umc.Pages;

public partial class WhatsNewPage : CustomPage
{
    public WhatsNewPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<WhatsNewViewModel>();
    }

    public WhatsNewPage(WhatsNewViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
