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
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

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
		public ReleaseAddress		Address;
		public string				Channel;		/// stable, beta, nightly, debug,...
		public Version				PreviousVersion;
		public byte[]				CompleteHash;
		public long					CompleteSize;
		public ReleaseAddress[]		CompleteDependencies;
		public Version				IncrementalMinimalVersion;
		public byte[]				IncrementalHash;
		public long					IncrementalSize;
		public ReleaseAddress[]		IncrementalDependencies;

		public override bool		Valid => Address.Valid;
		public override string		Description => $"{Address}/{Channel}";

		byte[]						Hash;
		bool						Archived;

		public const string			CompleteSizeField = "CompleteSize";
		public const string			CompleteHashField = "CompleteHash";
		public const string			IncrementalSizeField = "IncrementalSize";
		public const string			IncrementalHashField = "IncrementalHash";

		public ReleaseRegistration()
		{
		}

		public ReleaseRegistration(	PrivateAccount				signer, 
									ReleaseAddress				address, 
									string						channel, 
									Version						previous, 
									long						completesize,
									byte[]						completehash,
									IEnumerable<ReleaseAddress>	completedependencies, 
									Version						incrementalminimalversion, 
									long						incrementalsize,
									byte[]						incrementalhash,
									IEnumerable<ReleaseAddress>	incrementaldependencies)
		{
			Signer	= signer;
			Address = address;
			Channel = channel;
			PreviousVersion = previous;
			
			CompleteSize			= completesize;
			CompleteHash			= completehash;
			CompleteDependencies	= completedependencies.ToArray();
			
			IncrementalMinimalVersion	= incrementalminimalversion;
			IncrementalHash				= incrementalhash;
			IncrementalSize				= incrementalsize;
			IncrementalDependencies		= incrementaldependencies.ToArray();
		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			d.Add("Address").Value = Address;
			d.Add("Channel").Value = Channel;
			d.Add("PreviousVersion").Value = PreviousVersion;
	
			if(!Archived)
			{
				d.Add(CompleteHashField).Value = CompleteHash;
				d.Add(CompleteSizeField).Value = CompleteSize;
	
				if(CompleteDependencies.Any())
				{
					var cd = d.Add("CompleteDependencies");
					foreach(var i in CompleteDependencies)
					{
						cd.Add(i.ToString());
					}
				}
	
				if(IncrementalSize > 0)
				{
					d.Add("IncrementalMinimalVersion").Value = IncrementalMinimalVersion;
					d.Add(IncrementalHashField).Value = IncrementalHash;
					d.Add(IncrementalSizeField).Value = IncrementalSize;
		
					if(IncrementalDependencies.Any())
					{
						var id = d.Add("IncrementalDependencies");
						foreach(var i in IncrementalDependencies)
						{
							id.Add(i.ToString());
						}
					}
				}
			}

			d.Add("Hash").Value = Hash;

			return d;		
		}

		public byte[] GetOrCalcHash()
		{
			if(Hash != null)
			{
				return Hash;
			}

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
	
			w.Write(Address);
			w.WriteUtf8(Channel);
			w.Write(PreviousVersion);
			w.Write(CompleteHash);
			w.Write7BitEncodedInt64(CompleteSize);
			w.Write(CompleteDependencies);
			
			w.Write7BitEncodedInt64(IncrementalSize);

			if(IncrementalSize > 0)
			{
				w.Write(IncrementalMinimalVersion);
				w.Write(IncrementalHash);
				w.Write(IncrementalDependencies);
			}
				
			Hash = Cryptography.Current.Hash(s.ToArray());
		
			return Hash;
		}

		public override void HashWrite(BinaryWriter writer)
		{
			writer.Write(GetOrCalcHash());
		}

		public override void WritePaid(BinaryWriter w)
		{
			w.Write(GetOrCalcHash());
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Address = r.Read<ReleaseAddress>();
			Channel = r.ReadUtf8();
			PreviousVersion = r.ReadVersion();

			Archived = r.ReadBoolean();

			if(Archived)
			{
				Hash = r.ReadSha3();
			} 
			else
			{
				CompleteSize = r.Read7BitEncodedInt64();
				CompleteHash = r.ReadSha3();
				CompleteDependencies = r.ReadArray<ReleaseAddress>();

				IncrementalSize = r.Read7BitEncodedInt64();
				
				if(IncrementalSize > 0)
				{
					IncrementalMinimalVersion = r.ReadVersion();
					IncrementalHash = r.ReadSha3();
					IncrementalDependencies = r.ReadArray<ReleaseAddress>();
				}

				Hash = GetOrCalcHash();
			}
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Address);
			w.WriteUtf8(Channel);
			w.Write(PreviousVersion);

			w.Write(Archived);

			if(Archived)
			{
				w.Write(GetOrCalcHash());
			} 
			else
			{
				w.Write7BitEncodedInt64(CompleteSize);
				w.Write(CompleteHash);
				w.Write(CompleteDependencies);
			
				w.Write7BitEncodedInt64(IncrementalSize);

				if(IncrementalSize > 0)
				{
					w.Write(IncrementalMinimalVersion);
					w.Write(IncrementalHash);
					w.Write(IncrementalDependencies);
				}
			}
		}

		public override void Execute(Roundchain chain, Round round)
		{
			if(Archived)
				return;

			var a = round.FindAuthor(Address.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(!a.Products.Contains(Address.Product))
			{
				Error = "Product is not registered";
				return;
			}
 
			var p = round.FindProduct(Address);

			if(p == null)
				throw new IntegrityException("Product not found");
			
			var z = p.Realizations.Find(i => i.Name == Address.Platform);

			if(z == null)
			{
				Error = "Realization not found";
				return;
			}
	
			var r = p.Releases.FirstOrDefault(i => i.Platform == Address.Platform && i.Channel == Channel);
					
			if(r != null)
			{
				if(r.Version < Address.Version)
				{
					var prev = chain.FindRound(r.Rid).FindOperation<ReleaseRegistration>(m =>	m.Address.Author == Address.Author && 
																								m.Address.Product == Address.Product && 
																								m.Address.Platform == Address.Platform && 
																								m.Channel == Channel);
					if(prev == null)
						throw new IntegrityException("No ReleaseRegistration found");
					
					p = round.ChangeProduct(Address);

					prev.Archived = true;
					round.AffectedRounds.Add(prev.Transaction.Payload.Round);
					p.Releases.Remove(r);

				} 
				else
				{
					Error = "Version must be greater than current";
					return;
				}
			}
			else
				p = round.ChangeProduct(Address);


			//var rls = round.GetReleases(round.Id);
			//rls.Add(this);

			p.Releases.Add(new ReleaseEntry(Address.Platform, Address.Version, Channel, round.Id));
		}
	}
}