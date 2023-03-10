using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Sun.FUI
{
	public partial class ProductPanel : MainPanel
	{
		public ProductPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
				BindAccounts(SearchAccount);
				BindAccounts(PublisherAccount);
				
				registeringProduct_TextChanged(null, null);
			}
		}

		private void search_Click(object sender, EventArgs e)
		{
			try
			{
				products.Items.Clear();
				
				foreach(var ar in FindProducts(Account.Parse(SearchAccount.Text)))
				{
					var i = new ListViewItem(ar.Product.Address.Product);
					i.Tag = ar;
					i.SubItems.Add(ar.Product.Title);
					i.SubItems.Add(ar.Author.Name);
	//				i.SubItems.Add(ar.Publisher.ToString());
				
					products.Items.Add(i);
				}
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
		
		private void register_Click(object sender, EventArgs e)
		{
			try
			{
				if(!Operation.IsValid(ProductName.Text, ProductTitle.Text))
					throw new ArgumentException("Invalid product name");

				if(Author.SelectedItem == null)
					throw new ArgumentException("Invalid author name. If you don't own any author yet then you need to register one first.");

				var a = Core.Database.Authors.Find(Author.SelectedItem as string, int.MaxValue);

				Core.Enqueue(new ProductRegistration(	GetPrivate(a.Owner),
														new ProductAddress(ProductName.Text, Author.SelectedItem as string),
														ProductTitle.Text),
														PlacingStage.Null,
														null);
			}
			catch(Exception ex) when (ex is RequirementException || ex is ArgumentException)
			{
				ShowError(ex.Message);
			}
		}

		private void registeringProduct_TextChanged(object sender, EventArgs e)
		{
			if(ProductTitle.Text.Length > 0)
			{
				ProductName.Text = Operation.TitleToName(ProductTitle.Text);
			}
		}

		private void RemovePublisher_Click(object sender, EventArgs e)
		{
			Publishers.Items.Remove(Publishers.SelectedItem);
		}

		private void AddPublisher_Click(object sender, EventArgs e)
		{
			if(!Publishers.Items.Contains(PublisherAccount.SelectedItem))
			{
				Publishers.Items.Add(PublisherAccount.SelectedItem);
			}
		}

		private void Change_Click(object sender, EventArgs e)
		{

		}

		private void products_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			Management.Enabled = e.IsSelected && Vault.Accounts.Contains((e.Item.Tag as ProductModel).Author.Owner);
		}
	}
}
