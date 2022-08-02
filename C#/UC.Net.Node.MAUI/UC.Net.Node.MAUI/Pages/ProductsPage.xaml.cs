namespace UC.Net.Node.MAUI.Pages;

    public partial class ProductsPage : CustomPage
    {
        public ProductsPage()
        {
            InitializeComponent();
            BindingContext = new ProductsViewModel(ServiceHelper.GetService<ILogger<ProductsViewModel>>());
        }
    }
    public partial class ProductsViewModel : BaseViewModel
    {
        public ProductsViewModel(ILogger<ProductsViewModel> logger) : base(logger)
        {
            ProductsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };
            Products.Add(new Product {Name = "Ultranet User Center", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "Aximion3D", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "Ultranet User Center", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "3D UI", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "Ultranet User Node", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "Aximion3D", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "3D UI", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "Ultranet User Node", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "Aximion3D", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "3D UI", Owner = "ultranetorg" });
            Products.Add(new Product { Name = "Ultranet User Node", Owner = "ultranetorg" });
        }

		[RelayCommand]
        private async void Create()
        {
            await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
        }
	
		[RelayCommand]
        private async void Restore()
        {
            await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
        }

		[RelayCommand]
        private void ItemTapped(Product Product)
        {  
        }

		[RelayCommand]
        private async void Options(Wallet wallet)
        {
			// has been changes from Product to Wallet
           await AccountOptionsPopup.Show(wallet);
        }

        Product _SelectedItem ;
        public Product SelectedItem
        {
            get { return _SelectedItem; }
            set { SetProperty(ref _SelectedItem, value); }
        }
        CustomCollection<Product> _Products =new CustomCollection<Product>();
        public CustomCollection<Product>  Products
        {
            get { return _Products; }
            set { SetProperty(ref _Products, value); }
        }
        CustomCollection<string> _ProductsFilter = new CustomCollection<string>();
        public CustomCollection<string> ProductsFilter
        {
            get { return _ProductsFilter; }
            set { SetProperty(ref _ProductsFilter, value); }
        }
    }
