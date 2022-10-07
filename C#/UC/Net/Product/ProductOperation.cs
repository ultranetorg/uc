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
		public override bool	Valid => 0 < Address.Author.Length && 0 < Address.Product.Length;

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

		public override void Execute(Roundchain chain, Round round)
		{
			var a = round.FindAuthor(Address.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}


			if(!a.Products.Contains(Address.Product))
			{
				a = round.ChangeAuthor(Address.Author);
				///a.Rid = round.Id;
				a.Products.Add(Address.Product);
			}
			 
			var p = round.ChangeProduct(Address);
		
			p.Title				= Title;
			p.LastRegistration	= round.Id;
		}
	}

	public class RealizationRegistration : Operation
	{
		public RealizationAddress			Address;
		public OsBinaryIdentifier[]			OSes;
		public override string				Description => $"{Address}";
		public override bool				Valid => Address.Valid;

		public RealizationRegistration()
		{
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			Address	= r.Read<RealizationAddress>();
			OSes	= r.ReadArray<OsBinaryIdentifier>();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);
			w.Write(Address);
			w.Write(OSes);
		}

		public override void Execute(Roundchain chain, Round round)
		{
			var a = round.FindAuthor(Address.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			//if(!a.Products.Contains(Address.Product))
			//{
			//	a = round.ChangeAuthor(Address.Author);
			//	///a.Rid = round.Id;
			//	a.Products.Add(Address.Product);
			//}
			 
			var p = round.ChangeProduct(Address);
			
			p.Realizations.RemoveAll(i => i.Name == Address.Platform);
			p.Realizations.Add(new RealizationEntry{Name = Address.Platform, OSes = OSes});
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
		public Manifest				Manifest;

		public override bool		Valid => true;
		public override string		Description => $"{Manifest.Address}/{Manifest.Channel}";

		public ReleaseRegistration()
		{
		}

		public ReleaseRegistration(PrivateAccount signer, Manifest manifest)
		{
			Signer	= signer;
			Manifest = manifest;
		}

		public override void HashWrite(BinaryWriter writer)
		{
			writer.Write(Manifest);
		}

		public override void WritePaid(BinaryWriter writer)
		{
			writer.Write(Manifest);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Manifest = reader.Read<Manifest>();
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Manifest);
		}

		public override void Execute(Roundchain chain, Round round)
		{
			//if(Manifest.Archived)
			//	return;

			var a = round.FindAuthor(Manifest.Address.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(!a.Products.Contains(Manifest.Address.Product))
			{
				Error = "Product not found";
				return;
			}
 
			var p = round.FindProduct(Manifest.Address);

			if(p == null)
				throw new IntegrityException("ProductEntry not found");
			
			var z = p.Realizations.Find(i => i.Name == Manifest.Address.Platform);

			if(z == null)
			{
				Error = "Realization not found";
				return;
			}

			bool checkrlz(ReleaseAddress d)
			{
				var a = round.FindAuthor(d.Author);
				
				if(a == null)
					return false;

				if(!a.Products.Contains(d.Product))
					return false;
			
				var p = round.FindProduct(d);

				if(p == null)
					throw new IntegrityException("ProductEntry not found");

				var z = p.Realizations.Find(i => i.Name == d.Platform);
				
				if(z == null)
					return false;

				var r = p.Releases.Where(i => i.Platform == d.Platform && i.Version == d.Version);

				return r != null;
			}

			foreach(var i in Manifest.AddedCoreDependencies)
			{
				if(checkrlz(i) == false)
				{
					Error = "Incorrect AddedCoreDependencies";
					return;
				}
			}

			foreach(var i in Manifest.RemovedCoreDependencies)
			{
				if(checkrlz(i) == false)
				{
					Error = "Incorrect RemovedCoreDependencies";
					return;
				}
			}
	
			var ce = p.Releases.Where(i => i.Platform == Manifest.Address.Platform).MaxBy(i => i.Version);
					
			if(ce != null)
			{
				if(ce.Version < Manifest.Address.Version)
				{
				//	var prev = chain.FindRound(r.Rid).FindOperation<ReleaseRegistration>(m =>	m.Manifest.Address.Author == Manifest.Address.Author && 
				//																				m.Manifest.Address.Product == Manifest.Address.Product && 
				//																				m.Manifest.Address.Platform == Manifest.Address.Platform && 
				//																				m.Manifest.Channel == Manifest.Channel);
				//	if(prev == null)
				//		throw new IntegrityException("No ReleaseRegistration found");
				//	
					p = round.ChangeProduct(Manifest.Address);
				//
				//	prev.Manifest.Archived = true;
				//	round.AffectedRounds.Add(prev.Transaction.Payload.Round);
				//	p.Releases.Remove(r);
				} 
				else
				{
					Error = "Version must be greater than current";
					return;
				}
			}
			else
				p = round.ChangeProduct(Manifest.Address);
			
			var e = new ReleaseEntry(Manifest.Address.Platform, Manifest.Address.Version, Manifest.Channel, round.Id);

			//if(ce != null)
			//{
			//	e.MergedDependencies = new List<ReleaseAddress>(ce.MergedDependencies);
			//}
			//
			//foreach(var i in Manifest.AddedCoreDependencies)
			//	e.MergedDependencies.Add(i);
			//
			//foreach(var i in Manifest.RemovedCoreDependencies)
			//	e.MergedDependencies.Remove(i);

			p.Releases.Add(e);
		}
	}
}