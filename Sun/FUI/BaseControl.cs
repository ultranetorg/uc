using System.Collections;
using System.Windows.Forms;

namespace Uccs.Sun.FUI
{
	// 	public class ProductModel
	// 	{
	// 		public ProductEntry Product;
	// 		public AuthorEntry Domain;
	// 
	// 		public ProductModel()
	// 		{
	// 		}
	// 	}

	public class BaseControl : UserControl
	{
		protected readonly Net.Sun	Sun;
		protected Mcv				Mcv;
		protected Rds				Rds => Mcv as Rds;

		public BaseControl()
		{
		}

		public BaseControl(Net.Sun d)
		{
			Sun = d;
		}

		public BaseControl(Mcv d)
		{
			Mcv = d;
			Sun = d.Sun;
		}

		public IEnumerable<DomainEntry> FindAuthors(AccountAddress owner)
		{
			var o = new List<DomainEntry>();
			
			foreach(var r in Mcv.Tail.Cast<RdsRound>())
				foreach(var a in r.AffectedDomains)
					if(a.Value.Owner == owner && !o.Any(i => i.Address == a.Key))
					{
						o.Add(a.Value);
					}

			/// TODO: too slow
			o.AddRange((Mcv as Rds).Domains.Where(i => i.Owner == owner));

			return o;
		}

// 		public IEnumerable<ProductModel> FindProducts(AccountAddress owner)
// 		{
// 			var o = new List<ProductModel>();
// 			
// 			foreach(var r in Database.Tail)
// 				foreach(var p in r.AffectedProducts)
// 				{
// 					foreach(var rx in Database.Tail)
// 						foreach(var a in rx.AffectedAuthors)
// 							if(a.Value.Owner == owner && p.Key.Domain == a.Key && !o.Any(i => i.Domain.Name == a.Key && i.Product.Address == p.Key))
// 							{
// 								o.Add(new ProductModel{Product = p.Value, Domain = a.Value});
// 							}
// 				}
// 
// 			/// TODO: too slow
// 			foreach(var i in Database.Domains.Where(i => i.Owner == owner)
// 											 .SelectMany(a => Database.Products.Where(i => i.Address.Domain == a.Name).Select(p =>	new ProductModel{Product = p, Domain = a})))
// 			{
// 					if(!o.Any(x => x.Domain.Name == i.Domain.Name && x.Product.Address == i.Product.Address))
// 					{
// 						o.Add(i);
// 					}
// 			}
// 
// 			return o;
// 		}

		protected void FillAccounts(ComboBox b)
		{
			b.Items.Clear();
	
			IEnumerable<AccountAddress> keys;

			lock(Sun.Lock)
				keys = Sun.Vault.Wallets.Keys.ToArray();

			foreach(var i in keys)
				b.Items.Add(i);
		
			if(b.Items.Count > 0)
				b.SelectedIndex = 0;
		}

		public void BindAccounts(ComboBox b, Action filled = null)
		{
			Sun.Vault.AccountsChanged += () => {
													BeginInvoke(new Action(() => { 
																				FillAccounts(b);
																				filled?.Invoke();
																			}));
												};
			FillAccounts(b);
			filled?.Invoke();
		}

// 		public void BindProducts(ComboBox b)
// 		{
// 			void fill()
// 			{
// 				b.Items.Clear();
// 	
// 				lock(Core.Lock)
// 				{
// 					foreach(var a in Vault.Accounts)
// 						foreach(var p in FindProducts(a))
// 							b.Items.Add(p.Product);
// 				}
// 	
// 				if(b.Items.Count > 0)
// 					b.SelectedIndex = 0;
// 			}
// 
// 			Core.Database.BlockAdded +=	b =>{
// 												if(b is Payload p && p.Transactions.Any(i => Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is ProductRegistration)))
// 												{
// 													BeginInvoke((MethodInvoker)delegate{ fill(); });
// 												}
// 											};
// 			fill();
// 		}
 
// 		public void BindAuthors(ComboBox b, Action filled = null)
// 		{
// 			void fill()
// 			{
// 				b.Items.Clear();
// 	
// 				lock(Core.Lock)
// 				{
// 					foreach(var i in Vault.Accounts)
// 						foreach(var p in FindAuthors(i))
// 							b.Items.Add(p.Name);
// 				}
// 	
// 				if(b.Items.Count > 0)
// 					b.SelectedIndex = 0;
// 			
// 				filled?.Invoke();
// 			}
// 
// 			Core.Database.BlockAdded += b => {
// 													if(b is Payload p && (	p.Transactions.Any(i => Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is AuthorRegistration || o is AuthorTransfer)) ||
// 																			p.Transactions.Any(i => !Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is AuthorTransfer at && Vault.Accounts.Contains(at.To))) 
// 																			))
// 													{
// 														BeginInvoke((MethodInvoker)delegate{ fill(); });
// 													}
// 												};
// 			fill();
// 		}

		public void ShowException(string message, Exception ex)
		{
			MessageBox.Show(this, message + " (" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public void ShowError(string message)
		{
			MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public AccountKey GetPrivate(AccountAddress account)
		{
			if(!Sun.Vault.IsUnlocked(account))
			{
				var pa = new EnterPasswordForm(SunGlobals.Secrets.Password);
	
				if(pa.Ask($"A password required to access {account} account"))
				{
					try
					{
						return Sun.Vault.Unlock(account, pa.Password);
					}
					catch(Exception ex)
					{
						throw new Exception($"Wallet access failed.\nThe password is incorrect or wallet file is invalid.\n({ex.Message})", ex);
					}
				}
			}

			return Sun.Vault.GetKey(account);
		}

		public static string Dump(XonDocument doc)
		{
			string t = "";
			doc.Dump((n, l) => t += new string(' ', l * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<String>(n, n.Value))) + Environment.NewLine);
			return t;
		}


		public void Dump(object o, Control fields, Control values)
		{
			void save(string name, Type type, object value, int tab)
			{
				fields.Text += new string(' ', tab * 4) + $"{name}\n";

				if(type.GetInterfaces().Any(i => i == typeof(ICollection)))
				{
					values.Text += $"{{{(value as ICollection)?.Count}}}\n";
				}
				else
				{
					values.Text += $"{value?.ToString()}\n";
				}
			}

			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				save(i.Name, i.PropertyType, i.GetValue(o), 1);
			}
		}
	}

	public class MainPanel : BaseControl
	{
		public bool First = true;

		public virtual void Open(bool first){ }
		public virtual void Close(){ }
		public virtual void PeriodicalRefresh(){ }

		public MainPanel()
		{
		}

		public MainPanel(Net.Sun d) : base(d)
		{
		}

		public MainPanel(Mcv d) : base(d)
		{
		}
	}
}
