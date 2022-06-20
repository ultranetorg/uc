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
																			Payloads = new(), 
																			Violators = new(), 
																			Joiners = new(), 
																			Leavers = new(), 
																			HubJoiners = new(), 
																			HubLeavers = new(), 
																			FundJoiners = new(), 
																			FundLeavers = new(),
																		  };

		public List<byte[]>		Payloads;
		public List<byte[]>		Joiners;
		public List<byte[]>		Leavers;
		public List<byte[]>		HubJoiners;
		public List<byte[]>		HubLeavers;
		public List<byte[]>		Violators;
		public List<byte[]>		FundJoiners;
		public List<byte[]>		FundLeavers;
		public ChainTime		Time;

		public void WriteHashable(BinaryWriter w)
		{
			Write(w);
		}

		public void Write(BinaryWriter w)
		{
			if(Payloads.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong Payloads Prefix length");
			if(Violators.Any(i => i.Length != PrefixLength))			throw new IntegrityException("Wrong Violators Prefix length");
			if(Joiners.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong HubJoiners Prefix length");
			if(Leavers.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong HubLeavers Prefix length");
			if(HubJoiners.Any(i => i.Length != PrefixLength))			throw new IntegrityException("Wrong HubJoiners Prefix length");
			if(HubLeavers.Any(i => i.Length != PrefixLength))			throw new IntegrityException("Wrong HubLeavers Prefix length");
			if(FundJoiners.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableAssignments Prefix length");
			if(FundLeavers.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableRevocations Prefix length");

			w.Write(Payloads,				i => w.Write(i));
			w.Write(Violators,				i => w.Write(i));
			w.Write(Joiners,				i => w.Write(i));
			w.Write(Leavers,				i => w.Write(i));
			w.Write(HubJoiners,				i => w.Write(i));
			w.Write(HubLeavers,				i => w.Write(i));
			w.Write(FundJoiners,	i => w.Write(i));
			w.Write(FundLeavers,	i => w.Write(i));
			w.Write(Time);
		}

		public void Read(BinaryReader r)
		{
			Payloads			= r.ReadList(() => r.ReadBytes(PrefixLength));
			Violators			= r.ReadList(() => r.ReadBytes(PrefixLength));
			Joiners				= r.ReadList(() => r.ReadBytes(PrefixLength));
			Leavers				= r.ReadList(() => r.ReadBytes(PrefixLength));
			HubJoiners			= r.ReadList(() => r.ReadBytes(PrefixLength));
			HubLeavers			= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundJoiners	= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundLeavers	= r.ReadList(() => r.ReadBytes(PrefixLength));
			Time				= r.ReadTime();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as RoundReference);
		}

		public override int GetHashCode()
		{
			return	Payloads.Count.GetHashCode() ^ 
					Violators.Count.GetHashCode() ^ 
					Joiners.Count.GetHashCode() ^ 
					Leavers.Count.GetHashCode() ^ 
					HubJoiners.Count.GetHashCode() ^ 
					HubLeavers.Count.GetHashCode() ^ 
					FundJoiners.Count.GetHashCode() ^ 
					FundLeavers.Count.GetHashCode() ^
					Time.Ticks.GetHashCode();
		}

		public bool Equals(RoundReference o)
		{
			return	Payloads			.SequenceEqual(o.Payloads,				new BytesEqualityComparer()) &&
					Violators			.SequenceEqual(o.Violators,				new BytesEqualityComparer()) &&
					Joiners				.SequenceEqual(o.Joiners,				new BytesEqualityComparer()) &&
					Leavers				.SequenceEqual(o.Leavers,				new BytesEqualityComparer()) &&
					HubJoiners			.SequenceEqual(o.HubJoiners,			new BytesEqualityComparer()) &&
					HubLeavers			.SequenceEqual(o.HubLeavers,			new BytesEqualityComparer()) &&
					FundJoiners	.SequenceEqual(o.FundJoiners,	new BytesEqualityComparer()) &&
					FundLeavers	.SequenceEqual(o.FundLeavers,	new BytesEqualityComparer()) &&
					Time == o.Time;
		}
	}
}
