﻿using static Microsoft.Maui.Controls.Compatibility.Grid;
namespace UC.Umc.Controls;

public class MultiFieldsEntry : Grid
{
	#region Bindable Properties

	public static BindableProperty FieldsProperty = BindableProperty.Create(nameof(Fields), typeof(int), typeof(BorderEditor), 4);

	public int Fields
	{
		get { return (int)GetValue(FieldsProperty); }
		set { SetValue(FieldsProperty, value); }
	}

	public static BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(MultiFieldsEntry), Colors.Transparent);

	public Color BorderColor
	{
		get { return (Color)GetValue(BorderColorProperty); }
		set { SetValue(BorderColorProperty, value); }
	}

	public static BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(MultiFieldsEntry), Colors.White);

	public Color FillColor
	{
		get { return (Color)GetValue(FillColorProperty); }
		set { SetValue(FillColorProperty, value); }
	}

	public static BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(float), typeof(MultiFieldsEntry), 1f);

	public float BorderWidth
	{
		get { return (float)GetValue(BorderWidthProperty); }
		set { SetValue(BorderWidthProperty, value); }
	}

	public static BindableProperty BorderRadiusProperty = BindableProperty.Create(nameof(BorderRadius), typeof(float), typeof(MultiFieldsEntry), 1f);

	public float BorderRadius
	{
		get { return (float)GetValue(BorderRadiusProperty); }
		set { SetValue(BorderRadiusProperty, value); }
	}

	public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MultiFieldsEntry), Colors.Black);

	public Color TextColor
	{
		get { return (Color)GetValue(TextColorProperty); }
		set { SetValue(TextColorProperty, value); }
	}

	public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(MultiFieldsEntry), (double)16);

	public double FontSize
	{
		get { return (double)GetValue(FontSizeProperty); }
		set { SetValue(FontSizeProperty, value); }
	}

	public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MultiFieldsEntry), 
		null, BindingMode.OneWay, propertyChanged: TextPropertyChanged);

	public string Text
	{
		get { return (string)GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}

	private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var grid = bindable as MultiFieldsEntry;
		if (grid.Children.Count >= grid.Text.Length)
		{
			for (var i = 0; i < grid.Text.Length; i++)
			{
				(grid.Children[i] as BorderEntry).Text = grid.Text[i].ToString();
			}
		}
	}

	public static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(MultiFieldsEntry));

	public string Placeholder
	{
		get { return (string)GetValue(PlaceholderProperty); }
		set { SetValue(PlaceholderProperty, value); }
	}

	#endregion Bindable Properties

	public MultiFieldsEntry()
    {
		Initialization();
	}

	private void Initialization()
	{
		Children.Clear();
		ColumnDefinitions.Clear();
		ColumnSpacing = 10;
		FlowDirection = FlowDirection.LeftToRight;

		for (var i = 0; i < Fields; i++)
		{
			var entry = new BorderEntry
			{
				FillColor = FillColor,
				BorderColor = BorderColor,
				BorderRadius = BorderRadius,
				BorderWidth = BorderWidth,
				TextColor = TextColor,
				FontSize = FontSize,
				Placeholder = Placeholder,
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = TextAlignment.Center,
				Keyboard = Keyboard.Numeric,
			};
			entry.TextChanged += Entry_TextChanged;
			ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
			((IGridList<View>)Children).Add(entry, ColumnDefinitions.Count-1, 0 );
		}
		(Children[0] as BorderEntry).Focus();
	}

	// TBR
    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            bool isValid = e.NewTextValue.ToCharArray().All(x => char.IsDigit(x));
            ((Entry)sender).Text = isValid ? e.NewTextValue : e.NewTextValue.Remove(e.NewTextValue.Length - 1);
        }
        if (e.NewTextValue.Length > 0)
        {
            var nextindex = Children.IndexOf(sender as BorderEntry) + 1;
            if (Children.Count > nextindex)
            {
                var nextenrty = Children[nextindex] as BorderEntry;
                nextenrty.Focus();
            }
        }
        if (e.NewTextValue.Length > 1)
        {
            (sender as BorderEntry).Text = e.NewTextValue.Substring(1, 1);
        }
        if (e.NewTextValue.Length == 0)
        {
            var preIndex = Children.IndexOf(sender as BorderEntry) - 1;
            if (preIndex > -1)
            {
                var prevenrty = Children[preIndex] as BorderEntry;
                prevenrty.Focus();
            }
        }
        Text = string.Empty;
        foreach (var entry in Children)
        {
            Text += (entry as BorderEntry).Text;
        }
    }
}