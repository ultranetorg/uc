using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using UC.Net;

namespace UC.Net
{
	public class ProductRegistration : Operation
	{
		public ProductAddress	Address;
		public string			Title;
		public override string	Description => $"{Address} as {Title}";
		public override bool	Valid => IsValid(Address.Product, Title);

		public ProductRegistration()
		{
		}

		public ProductRegistration(PrivateAccount signer, ProductAddress name, string title)
		{
			Signer		= signer;
			Address		= name;
			Title		= title;
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Address	= r.Read<ProductAddress>();
			Title	= r.ReadUtf8();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Address);
			w.WriteUtf8(Title);
		}

		public override void Execute(Database chain, Round round)
		{
			var a = round.FindAuthor(Address.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(!a.Products.Contains(Address.Product))
			{
				a = round.AffectAuthor(Address.Author);

				a.Products.Add(Address.Product);

				var p = round.AffectProduct(Address);
		
				p.Title				= Title;
				p.LastRegistration	= round.Id;
			}
			else
			{
				Error = "Product already registered";
				return;
			}
			 
		}
	}

	public class RealizationRegistration : Operation
	{
		public RealizationAddress			Realization;
		public Osbi[]						OSes;
		public override string				Description => $"{Realization}";
		public override bool				Valid => Realization.Valid;

		public RealizationRegistration()
		{
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			Realization	= r.Read<RealizationAddress>();
			OSes		= r.ReadArray<Osbi>();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);
			w.Write(Realization);
			w.Write(OSes);
		}

		public override void Execute(Database chain, Round round)
		{
			var a = round.FindAuthor(Realization.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(!a.Products.Contains(Realization.Product))
			{
				Error = "Product not found";
				return;
			}
			 
			var p = round.AffectProduct(Realization);
			
			p.Realizations.RemoveAll(i => i.Name == Realization.Platform);
			p.Realizations.Add(new ProductEntryRealization{Name = Realization.Platform, OSes = OSes});

			var r = round.AffectRealization(Realization);

			r.OSes = OSes;
			r.LastRegistration = round.Id;
		}
	}

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

		public override void Read(BinaryReader r)
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

		public override void Write(BinaryWriter w)
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
	}

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

		public ReleaseRegistration(PrivateAccount signer, ReleaseAddress release, string channel, byte[] manifest)
		{
			Signer	= signer;
			Release = release;
			Channel = channel;
			Manifest = manifest;
		}

		public override void HashWrite(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Manifest);
			writer.WriteUtf8(Channel);
		}

		public override void WritePaid(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Manifest);
			writer.WriteUtf8(Channel);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Release = reader.Read<ReleaseAddress>();
			Manifest = reader.ReadSha3();
			Channel = reader.ReadUtf8();
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			WritePaid(writer);
		}

		public override void Execute(Database chain, Round round)
		{
			var a = round.FindAuthor(Release.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(!a.Products.Contains(Release.Product))
			{
				Error = "Product not found";
				return;
			}
 
			var p = round.FindProduct(Release);

			if(p == null)
				throw new IntegrityException("ProductEntry not found");
			
			var z = p.Realizations.Find(i => i.Name == Release.Platform);

			if(z == null)
			{
				Error = "Realization not found";
				return;
			}

			var ce = p.Releases.Where(i => i.Platform == Release.Platform).MaxBy(i => i.Version);
					
			if(ce != null)
			{
				if(ce.Version < Release.Version)
				{
					p = round.AffectProduct(Release);
				} 
				else
				{
					Error = "Version must be greater than current";
					return;
				}
			}
			else
				p = round.AffectProduct(Release);
			
			var e = new ProductEntryRelease(Release.Platform, Release.Version, Channel, round.Id);

			p.Releases.Add(e);

			var r = round.AffectRelease(Release);

			r.Manifest = Manifest;
			r.Channel = Channel;
			r.LastRegistration = round.Id;
		}
	}
}