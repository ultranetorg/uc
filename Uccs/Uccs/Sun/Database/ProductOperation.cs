using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public class ProductRegistration : Operation
	{
		public ProductAddress	Address;
		public string			Title;
		public override string	Description => $"{Address} as {Title}";
		public override bool	Valid => IsValid(Address.Name, Title);

		public ProductRegistration()
		{
		}

		public ProductRegistration(AccountAddress signer, ProductAddress name, string title)
		{
			Signer		= signer;
			Address		= name;
			Title		= title;
		}

		protected override void ReadConfirmed(BinaryReader r)
		{
			Address	= r.Read<ProductAddress>();
			Title	= r.ReadUtf8();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Title);
		}

		public override void Execute(Database chain, Round round)
		{
			var a = chain.Authors.Find(Address.Author, round.Id);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(chain.Products.Find(Address, round.Id) != null)
			{
				Error = "Product already registered";
				return;
			}
			 
			a = round.AffectAuthor(Address.Author);

			//a.Products.Add(Address.Product);

			var p = round.AffectProduct(Address);
		
			p.Title					= Title;
		}
	}

	public class PlatformRegistration : Operation
	{
		public RealizationAddress			Platform;
		//public Osbi[]						OSes;
		public override string				Description => $"{Platform}";
		public override bool				Valid => true;

		public PlatformRegistration()
		{
		}

		protected override void ReadConfirmed(BinaryReader r)
		{
			Platform	= r.Read<RealizationAddress>();
			//OSes		= r.ReadArray<Osbi>();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(Platform);
			//w.Write(OSes);
		}

		public override void Execute(Database chain, Round round)
		{
			var a = chain.Authors.Find(Platform.Product.Author, round.Id);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(chain.Products.Find(Platform.Product, round.Id) == null)
			{
				Error = "Product not found";
				return;
			}


// 
// 			if(chain.Products.Find(Platform.Product, round.Id) == null)
// 			{
// 				Error = "Product not found";
// 				return;
// 			}
			 
			//var p = round.AffectProduct(Platform.Product);
			
			//p.Realizations.RemoveAll(i => i.Name == Realization.Name);
			//p.Realizations.Add(new ProductEntryRealization{Name = Realization.Name, OSes = OSes});

			var r = round.AffectPlatform(Platform);

			//r.OSes = OSes;
		}
	}

/*
	public class ProductControl : Operation
	{
		enum Change
		{
			AddPublisher, RemovePublisher, SetStatus
		}

		public ProductAddress		Product;
		public string				Class; /// Application, Library, Component(Add-on/Plugin), Font, etc.
		public ProductAddress		Master; /// For Components
		public string				LogoAddress;
		Dictionary<Change, object>	Actions;
		public override string		Description => $"{Product} ...";

		public ProductControl()
		{
		}

		public ProductControl(PrivateAccount signer, ProductAddress product)
		{
			Signer		= signer;
			Product		= product;
			Actions		= new();
		}

		public override bool Valid => Product.Valid;

		public override string	ToString()							=> base.ToString() + $", {Product}";
		public void				AddPublisher(Account publisher)		=> Actions[Change.AddPublisher] = publisher;
		public void				RemovePublisher(Account publisher)	=> Actions[Change.RemovePublisher] = publisher;
		public void				SetStatus(bool active)				=> Actions[Change.SetStatus] = active;

		public override void ReadConfirmed(BinaryReader r)
		{
			base.Read(r);

			Product	= r.Read<ProductAddress>();
			Actions = r.ReadDictionary(() =>{
												var k = (Change)r.ReadByte();	
												var o = new KeyValuePair<Change, object>(k,	k switch {
																										Change.AddPublisher => r.ReadAccount(),
																										Change.RemovePublisher => r.ReadAccount(),
																										Change.SetStatus => r.ReadBoolean(),
																										_ => throw new IntegrityException("Wrong ProductControl.Change")
																									 });
												return o;
											});
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Product);
			w.Write(Actions, i =>	{
										w.Write((byte)i.Key);

										switch(i.Key)
										{
											case Change.AddPublisher:		w.Write(i.Value as Account); break;
											case Change.RemovePublisher:	w.Write(i.Value as Account); break;
											case Change.SetStatus:			w.Write((bool)i.Value); break;
										}
									});
		}
	}*/

	public class ReleaseRegistration : Operation
	{
		public ReleaseAddress		Release { get; set; }
		public byte[]				Manifest { get; set; }
		public string				Channel { get; set; }

		public override bool		Valid => true;
		public override string		Description => $"{Release} {Channel} {Hex.ToHexString(Manifest)}";

		public ReleaseRegistration()
		{
		}

		public ReleaseRegistration(AccountAddress signer, ReleaseAddress release, string channel, byte[] manifest)
		{
			Signer	= signer;
			Release = release;
			Channel = channel;
			Manifest = manifest;
		}

		protected override void ReadConfirmed(BinaryReader reader)
		{
			Release = reader.Read<ReleaseAddress>();
			Manifest = reader.ReadSha3();
			Channel = reader.ReadUtf8();
		}

		protected override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Manifest);
			writer.WriteUtf8(Channel);
		}

		public override void Execute(Database chain, Round round)
		{
			var a = chain.Authors.Find(Release.Product.Author, round.Id);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(chain.Products.Find(Release.Product, round.Id) == null)
			{
				Error = "Product not found";
				return;
			}

			if(chain.Realizations.Find(Release.Realization, round.Id) == null)
			{
				Error = "Realization not found";
				return;
			}

// 			if(chain.Products.Find(Release, round.Id) == null)
// 			{
// 				Error = "Product not found";
// 				return;
// 			}
//  
// 			var p = round.FindProduct(Release);
// 
// 			if(p == null)
// 				throw new IntegrityException("ProductEntry not found");
// 			
// 			var z = chain.Realizations.Find((RealizationAddress)Release, round.Id);
// 			//var z = p.Realizations.Find(i => i.Name == Release.Realization);
// 
// 			if(z == null)
// 			{
// 				Error = "Realization not found";
// 				return;
// 			}

			var ce = chain.Releases.Where(Release.Product.Author, Release.Product.Name, i => i.Address.Realization.Name == Release.Realization.Name, round.Id).MaxBy(i => i.Address.Version);
					
			if(ce != null && ce.Address.Version >= Release.Version)
			{
				Error = "Version must be greater than current";
				return;
			}
			
			//var e = new ProductEntryRelease(Release.Realization, Release.Version, Channel, round.Id);

			//p.Releases.Add(e);

			var r = round.AffectRelease(Release);

			r.Manifest = Manifest;
			r.Channel = Channel;
		}
	}
}