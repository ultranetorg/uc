using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class RoundReference : IEquatable<RoundReference>
	{
		public const byte		PrefixLength = 4;
		public static readonly	RoundReference Empty = new RoundReference {
																			//BaseHash = Cryptography.ZeroHash,
																			Payloads = new(), 
																			Violators = new(), 
																			Joiners = new(), 
																			Leavers = new(), 
																			FundJoiners = new(), 
																			FundLeavers = new(),
																		  };

		//public byte[]			BaseHash;
		public List<byte[]>		Payloads;
		public ChainTime		Time;
		public List<byte[]>		Joiners;
		public List<byte[]>		Leavers;
		public List<byte[]>		Violators;
		public List<byte[]>		FundJoiners;
		public List<byte[]>		FundLeavers;

		public void WriteHashable(BinaryWriter w)
		{
			Write(w);
		}

		public void Write(BinaryWriter w)
		{
			#if DEBUG
			//if(BaseHash.Length != Cryptography.HashSize)		throw new IntegrityException("Wrong BaseHash length");
			if(Payloads.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong Payloads Prefix length");
			if(Violators.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong Violators Prefix length");
			if(Joiners.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong HubJoiners Prefix length");
			if(Leavers.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong HubLeavers Prefix length");
			if(FundJoiners.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableAssignments Prefix length");
			if(FundLeavers.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableRevocations Prefix length");
			#endif

			//w.Write(BaseHash);
			w.Write(Time);
			w.Write(Payloads,		i => w.Write(i));
			w.Write(Violators,		i => w.Write(i));
			w.Write(Joiners,		i => w.Write(i));
			w.Write(Leavers,		i => w.Write(i));
			w.Write(FundJoiners,	i => w.Write(i));
			w.Write(FundLeavers,	i => w.Write(i));
		}

		public void Read(BinaryReader r)
		{
			//BaseHash	= r.ReadSha3();
			Time		= r.ReadTime();
			Payloads	= r.ReadList(() => r.ReadBytes(PrefixLength));
			Violators	= r.ReadList(() => r.ReadBytes(PrefixLength));
			Joiners		= r.ReadList(() => r.ReadBytes(PrefixLength));
			Leavers		= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundJoiners	= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundLeavers	= r.ReadList(() => r.ReadBytes(PrefixLength));
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as RoundReference);
		}

		public override int GetHashCode()
		{
			return	//BaseHash[0].GetHashCode() ^
					Time.Ticks.GetHashCode() ^
					Payloads.Count.GetHashCode() ^ 
					Violators.Count.GetHashCode() ^ 
					Joiners.Count.GetHashCode() ^ 
					Leavers.Count.GetHashCode() ^ 
					FundJoiners.Count.GetHashCode() ^ 
					FundLeavers.Count.GetHashCode();
		}

		public bool Equals(RoundReference o)
		{
			return	Time == o.Time &&
					//BaseHash	.SequenceEqual(o.BaseHash) &&
					Payloads	.SequenceEqual(o.Payloads,		new BytesEqualityComparer()) &&
					Violators	.SequenceEqual(o.Violators,		new BytesEqualityComparer()) &&
					Joiners		.SequenceEqual(o.Joiners,		new BytesEqualityComparer()) &&
					Leavers		.SequenceEqual(o.Leavers,		new BytesEqualityComparer()) &&
					FundJoiners	.SequenceEqual(o.FundJoiners,	new BytesEqualityComparer()) &&
					FundLeavers	.SequenceEqual(o.FundLeavers,	new BytesEqualityComparer());
		}
	}
}
