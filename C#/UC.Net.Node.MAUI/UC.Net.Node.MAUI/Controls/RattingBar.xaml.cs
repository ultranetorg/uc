using System.Windows.Input;

namespace UC.Net.Node.MAUI.Controls;

// TBR completely
public partial class RattingBar : ContentView
{
    public event EventHandler ItemTapped = delegate { };
    private ImageSource _emptyStarImage = string.Empty;
    private ImageSource _fillStarImage = string.Empty;
    private readonly Image _star1 = new ();
    private readonly Image _star2 = new ();
    private readonly Image _star3 = new ();
    private readonly Image _star4 = new ();
    private readonly Image _star5 = new ();

    public RattingBar()
    {
		InitializeComponent();
		FillFakeData();
    }

    #region Image Height Width Property

    public static readonly BindableProperty ImageHeightProperty = BindableProperty.Create(
		nameof(ImageHeight), typeof(double), typeof(RattingBar), BindingMode.TwoWay, propertyChanged: ImageHeightPropertyChanged);

    public double ImageHeight
    {
        get { return (double)GetValue(ImageHeightProperty); }
        set { SetValue(ImageHeightProperty, value); }
    }

    public static readonly BindableProperty ImageWidthProperty = BindableProperty.Create(
		nameof(ImageWidth), typeof(double), typeof(RattingBar), BindingMode.TwoWay, propertyChanged: ImageWidthPropertyChanged);

    public double ImageWidth
    {
        get { return (double)GetValue(ImageWidthProperty); }
        set { SetValue(ImageWidthProperty, value); }
    }

    private static void ImageHeightPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (RattingBar)bindable;
        if (control != null)
        {
            // Set all images height equal
            control._star1.HeightRequest = (double)newValue;
            control._star2.HeightRequest = (double)newValue;
            control._star3.HeightRequest = (double)newValue;
            control._star4.HeightRequest = (double)newValue;
            control._star5.HeightRequest = (double)newValue;
        }
    }

    private static void ImageWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (RattingBar)bindable;
        if (control != null)
        {
			// Set all images width equal
            control._star1.WidthRequest = (double)newValue;
            control._star2.WidthRequest = (double)newValue;
            control._star3.WidthRequest = (double)newValue;
            control._star4.WidthRequest = (double)newValue;
            control._star5.WidthRequest = (double)newValue;
        }
    }

    #endregion Image Height Width Property

    #region Horizontal Vertical Allignment
	
    public static new readonly BindableProperty HorizontalOptionsProperty = BindableProperty.Create(
		nameof(HorizontalOptions), typeof(LayoutOptions), typeof(RattingBar), BindingMode.TwoWay, propertyChanged: HorizontalOptionsPropertyChanged);

    public new LayoutOptions HorizontalOptions
    {
        get { return (LayoutOptions)GetValue(HorizontalOptionsProperty); }
        set { SetValue(HorizontalOptionsProperty, value); }
    }

    private static void HorizontalOptionsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (RattingBar)bindable;
        control.stkRattingbar.HorizontalOptions = (LayoutOptions)newValue;
    }

    public static new readonly BindableProperty VerticalOptionsProperty = BindableProperty.Create(
		nameof(VerticalOptions), typeof(LayoutOptions), typeof(RattingBar), BindingMode.TwoWay, propertyChanged: VerticalOptionsPropertyChanged);

    public new LayoutOptions VerticalOptions
    {
        get { return (LayoutOptions)GetValue(VerticalOptionsProperty); }
        set { SetValue(VerticalOptionsProperty, value); }
    }

    private static void VerticalOptionsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (RattingBar)bindable;
        control.stkRattingbar.VerticalOptions = (LayoutOptions)newValue;
    }

    #endregion Horizontal Vertical Allignment

    #region Command binding property

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(RattingBar));

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    // (TBR) this property is private becuase i don't wanna access it globally
    private static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
		nameof(CommandParameter), typeof(object), typeof(RattingBar), propertyChanged: CommandParameterPropertyChanged);

    private object CommandParameter
    {
        get { return GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }

    // on the change of command parameter replace empty star image with fillstar image
    private static void CommandParameterPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RattingBar control)
        {
            var selectedValue = (int)newValue;
            FillStars(selectedValue, control);
        }
    }

	#endregion Command binding property

	#region Fill Data

	// (TBR!) this function will replace empty star with fill star
	private static void FillStars(int selectedValue, RattingBar bar)
	{
		Guard.IsNotNull(bar);

		bar.SelectedStarValue = selectedValue;
		bar._star1.Source = bar._fillStarImage;
		bar._star2.Source = bar._emptyStarImage;
		bar._star3.Source = bar._emptyStarImage;
		bar._star4.Source = bar._emptyStarImage;
		bar._star5.Source = bar._emptyStarImage;
		switch (selectedValue)
		{
			case 1:
				break;
			case 2:
				bar._star2.Source = bar._fillStarImage;
				break;
			case 3:
				bar._star2.Source = bar._fillStarImage;
				bar._star3.Source = bar._fillStarImage;
				break;
			case 4:
				bar._star2.Source = bar._fillStarImage;
				bar._star3.Source = bar._fillStarImage;
				bar._star4.Source = bar._fillStarImage;
				break;
			case 5:
				bar._star2.Source = bar._fillStarImage;
				bar._star3.Source = bar._fillStarImage;
				bar._star4.Source = bar._fillStarImage;
				bar._star5.Source = bar._fillStarImage;
				break;
		}
	}

	private void FillFakeData()
	{
		AddGestureRecognizer(_star1, 1);
		AddGestureRecognizer(_star2, 2);
		AddGestureRecognizer(_star3, 3);
		AddGestureRecognizer(_star4, 4);
		AddGestureRecognizer(_star5, 5);
	}

	private void AddGestureRecognizer(Image star = null, int parameter = 0)
	{
		star ??= new Image();
		
		// this event will fire when you click on image (star)
		star.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = Command,
			CommandParameter = parameter != 0 ? parameter : CommandParameter
		});

		stkRattingbar.Children.Add(star);
	}

	#endregion Fill Data

	#region EmptyStar and fillstar property

	public static readonly BindableProperty EmptyStarImageProperty = BindableProperty.Create(
		nameof(EmptyStarImage), typeof(ImageSource), typeof(RattingBar), default(ImageSource), BindingMode.TwoWay, propertyChanged: EmptyStarImagePropertyChanged);

    public ImageSource EmptyStarImage
    {
        get { return (ImageSource)GetValue(EmptyStarImageProperty); }
        set { SetValue(EmptyStarImageProperty, value); }
    }

    private static void EmptyStarImagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (RattingBar)bindable;
        if (control != null)
        {
            control._emptyStarImage = (ImageSource)newValue;

            //default set to all images as emptyStar
            control._star1.Source = control._emptyStarImage;
            control._star2.Source = control._emptyStarImage;
            control._star3.Source = control._emptyStarImage;
            control._star4.Source = control._emptyStarImage;
            control._star5.Source = control._emptyStarImage;

			// TBR
            // when default SelectedStarValue is assign first and fillstariamge or emptystart image assign later then on the Property Change of
            // SelectedStar fillStartImage and emptyStart Image show emty string so to handle this here i write this logic
            // <customcontrol:RattingBar x:Name = "customRattingBar" ImageWidth = "25" ImageHeight = "25" SelectedStarValue = "1"
			// FillStarImage = "fillstar" EmptyStarImage = "emptystar" />

            if (control._fillStarImage != null)
            {
                FillStars(control.SelectedStarValue, control);
            }
        }
    }

    public static readonly BindableProperty FillStarImageProperty = BindableProperty.Create(
		nameof(FillStarImage), typeof(ImageSource), typeof(RattingBar), default(ImageSource), BindingMode.TwoWay, propertyChanged: FillStarImagePropertyChanged);

    public ImageSource FillStarImage
    {
        get { return (ImageSource)GetValue(FillStarImageProperty); }
        set { SetValue(FillStarImageProperty, value); }
    }

    private static void FillStarImagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RattingBar control)
        {
            control._fillStarImage = (ImageSource)newValue;

            if (control._emptyStarImage != null)
            {
                FillStars(control.SelectedStarValue, control);
            }
        }
    }

	#endregion EmptyStar and fillstar property

	#region Selected Star

	// This will return the selected star value and also you can set the default selected star
	public static readonly BindableProperty SelectedStarValueProperty = BindableProperty.Create(
		nameof(SelectedStarValue), typeof(int), typeof(RattingBar), defaultValue: 0, BindingMode.TwoWay, propertyChanged: SelectedStarValuePropertyChanged);

	private static void SelectedStarValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is RattingBar control)
		{
			control.SelectedStarValue = (int)newValue;

			// if the SelectedStarValue is assign first and later fillStarImage & EmptyStar assign   
			if (control._fillStarImage != null && control._emptyStarImage != null)
			{
				FillStars(control.SelectedStarValue, control);
			}
		}
	}

	public int SelectedStarValue
	{
		get { return (int)GetValue(SelectedStarValueProperty); }
		set { SetValue(SelectedStarValueProperty, value); }
	}

	#endregion Selected Star

	private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        // Handle the pan - TBR
        double width = _star1.Width;

        if (e.TotalX > 0)
        {
            FillStars(1, this);
            Command?.Execute(1);
        }
        if (e.TotalX > width)
        {
            FillStars(2, this);
            Command?.Execute(2);
        }
        if (e.TotalX > (width * 2))
        {
            FillStars(3, this);
            Command?.Execute(3);

        }
        if (e.TotalX > (width * 3))
        {
            FillStars(4, this);
            Command?.Execute(4);

        }
        if (e.TotalX > (width * 4))
        {
            FillStars(5, this);
            Command?.Execute(5);
        }
    }
}