﻿namespace Uuc.Pages;

public partial class DashboardPage : ContentPage
{
	// int count = 0;

	public DashboardPage(DashboardPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		//count++;

		//if (count == 1)
		//	CounterBtn.Text = $"Clicked {count} time";
		//else
		//	CounterBtn.Text = $"Clicked {count} times";

		//SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
