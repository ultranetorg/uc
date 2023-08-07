using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
// 	public class ProductRegistration : Operation
// 	{
// 		public ProductAddress	Address;
// 		public string			Title;
// 		public string			Mime;
// 		public override string	Description => $"{Address} as {Title}";
// 		public override bool	Valid => IsValid(Address.Name, Title);
// 
// 		public ProductRegistration()
// 		{
// 		}
// 
// 		public ProductRegistration(AccountAddress signer, ProductAddress name, string title)
// 		{
// 			Signer		= signer;
// 			Address		= name;
// 			Title		= title;
// 		}
// 
// 		protected override void ReadConfirmed(BinaryReader r)
// 		{
// 			Address	= r.Read<ProductAddress>();
// 			Title	= r.ReadUtf8();
// 		}
// 
// 		protected override void WriteConfirmed(BinaryWriter w)
// 		{
// 			w.Write(Address);
// 			w.WriteUtf8(Title);
// 		}
// 
// 		public override void Execute(Database chain, Round round)
// 		{
// 			var a = chain.Authors.Find(Address.Author, round.Id);
// 
// 			if(a == null || a.Owner != Signer)
// 			{
// 				Error = NotOwnerOfAuthor;
// 				return;
// 			}
// 
// 			if(chain.Products.Find(Address, round.Id) != null)
// 			{
// 				Error = "Product already registered";
// 				return;
// 			}
// 			 
// 			a = round.AffectAuthor(Address.Author);
// 
// 			//a.Products.Add(Address.Product);
// 
// 			var p = round.AffectProduct(Address);
// 		
// 			p.Title					= Title;
// 		}
// 	}

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

	public class ResourceUpdation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public byte[]				Data { get; set; }
		public ResourceFlags		Flags { get; set; }
		public Coin					AnalysisFee { get; set; }

		public override bool		Valid => !Flags.HasFlag(ResourceFlags.Child);
		public override string		Description => $"{Resource}, {Flags}, {(Data != null ? Hex.ToHexString(Data) : null)}";

		public ResourceUpdation()
		{
		}

		public ResourceUpdation(AccountAddress signer, ResourceAddress release, byte[] data, ResourceFlags flags)
		{
			Signer	= signer;
			Resource = release;
			Data = data;
			Flags = flags;
		}

		protected override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Flags = (ResourceFlags)reader.ReadByte();
			
			if(reader.ReadBoolean())
			{
				Data = reader.ReadBytes();
			}

			if(Flags.HasFlag(ResourceFlags.Analysable))
			{
				AnalysisFee = reader.ReadCoin();
			}
		}

		protected override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Flags);

			writer.Write(Data != null);

			if(Data != null)
			{
				writer.WriteBytes(Data);
			}

			if(Flags.HasFlag(ResourceFlags.Analysable))
			{
				writer.Write(AnalysisFee);
			}
		}

		public override void Execute(Chainbase chain, Round round)
		{
			var a = chain.Authors.Find(Resource.Author, round.Id);

			if(a == null || a.Owner != Signer)
			{
				Error = NotOwnerOfAuthor;
				return;
			}

			//if(chain.Products.Find(Release.Product, round.Id) == null)
			//{
			//	Error = ProductNotFound;
			//	return;
			//}

			var e = chain.Resources.Find(Resource, round.Id);
					
			if(e != null && e.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = CantChangeConstantResource;
				return;
			}

			if(e == null/* && !a.ConstantDirectories.Any(i => Resource.Resource.StartsWith(i))*/)
			{
				//Error = "No containing constant directory found";
				//return;

				round.AffectAccount(Signer).Balance -= CalculateSpaceFee(round.Factor);
			}

			var r = round.AffectRelease(Resource);

			r.Flags = Flags;

			if(Data != null)
			{
				r.Data = Data;
			}

			r.AnalysisStage = Flags.HasFlag(ResourceFlags.Analysable) ? AnalysisStage.Pending : AnalysisStage.NotRequested;

			if(Flags.HasFlag(ResourceFlags.Analysable))
			{
				round.AffectAccount(Signer).Balance -= AnalysisFee;

				r.AnalysisFee = AnalysisFee;
				r.RoundId = round.Id;
				r.AnalysisQuorumRid = 0;
			}
		}
	}
}