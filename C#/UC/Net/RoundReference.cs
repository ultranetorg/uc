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
		public static readonly	RoundReference Empty = new RoundReference {Hash = new byte[Cryptography.HashSize], Payloads = new(), Violators = new(), Joiners = new(), Leavers = new(), FundableAssignments = new(), FundableRevocations = new() };

		public byte[]			Hash;
		public List<byte[]>		Payloads;
		public List<byte[]>		Joiners;
		public List<byte[]>		Leavers;
		public List<byte[]>		Violators;
		public List<byte[]>		FundableAssignments;
		public List<byte[]>		FundableRevocations;
		public ChainTime		Time;


		public void WriteHashable(BinaryWriter w)
		{
			if(Hash.Length != Cryptography.HashSize)				throw new IntegrityException("Wrong PitchHash length");
			if(Payloads.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong Payloads Prefix length");
			if(Violators.Any(i => i.Length != PrefixLength))			throw new IntegrityException("Wrong Violators Prefix length");
			if(Joiners.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong Joiners Prefix length");
			if(Leavers.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong Leavers Prefix length");
			if(FundableAssignments.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableAssignments Prefix length");
			if(FundableRevocations.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableRevocations Prefix length");

			w.Write(Hash);

			foreach(var i in Payloads)
				w.Write(i);

			foreach(var i in Violators)
				w.Write(i);

			foreach(var i in Joiners)
				w.Write(i);

			foreach(var i in Leavers)
				w.Write(i);

			foreach(var i in FundableAssignments)
				w.Write(i);

			foreach(var i in FundableRevocations)
				w.Write(i);

			w.Write(Time);
		}

		public void Write(BinaryWriter w)
		{
			if(Hash.Length != Cryptography.HashSize)					throw new IntegrityException("Wrong PitchHash length");
			if(Payloads.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong Payloads Prefix length");
			if(Violators.Any(i => i.Length != PrefixLength))			throw new IntegrityException("Wrong Violators Prefix length");
			if(Joiners.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong Joiners Prefix length");
			if(Leavers.Any(i => i.Length != PrefixLength))				throw new IntegrityException("Wrong Leavers Prefix length");
			if(FundableAssignments.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableAssignments Prefix length");
			if(FundableRevocations.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong FundableRevocations Prefix length");

			w.Write(Hash);
			w.Write(Payloads, i => w.Write(i));
			w.Write(Violators, i => w.Write(i));
			w.Write(Joiners, i => w.Write(i));
			w.Write(Leavers, i => w.Write(i));
			w.Write(FundableAssignments, i => w.Write(i));
			w.Write(FundableRevocations, i => w.Write(i));
			w.Write(Time);
		}

		public void Read(BinaryReader r)
		{
			Hash			= r.ReadBytes(Cryptography.HashSize);
			Payloads			= r.ReadList(() => r.ReadBytes(PrefixLength));
			Violators			= r.ReadList(() => r.ReadBytes(PrefixLength));
			Joiners				= r.ReadList(() => r.ReadBytes(PrefixLength));
			Leavers				= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundableAssignments	= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundableRevocations	= r.ReadList(() => r.ReadBytes(PrefixLength));
			Time				= r.ReadTime();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as RoundReference);
		}

		public override int GetHashCode()
		{
			return	Hash[0] ^
					Payloads.Count.GetHashCode() ^ 
					Violators.Count.GetHashCode() ^ 
					Joiners.Count.GetHashCode() ^ 
					FundableAssignments.Count.GetHashCode() ^ 
					FundableRevocations.Count.GetHashCode() ^
					Time.Ticks.GetHashCode();
		}

		public bool Equals(RoundReference o)
		{
			return	Hash			.SequenceEqual(o.Hash) &&
					Payloads			.SequenceEqual(o.Payloads,				new BytesEqualityComparer()) &&
					Violators			.SequenceEqual(o.Violators,				new BytesEqualityComparer()) &&
					Joiners				.SequenceEqual(o.Joiners,				new BytesEqualityComparer()) &&
					Leavers				.SequenceEqual(o.Leavers,				new BytesEqualityComparer()) &&
					FundableAssignments	.SequenceEqual(o.FundableAssignments,	new BytesEqualityComparer()) &&
					FundableRevocations	.SequenceEqual(o.FundableRevocations,	new BytesEqualityComparer()) &&
					Time == o.Time;
		}
	}
}
