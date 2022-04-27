using System.Collections.Generic;
using System.IO;

namespace UC.Net
{
	public enum MessageType : byte
	{
		Null, ReleaseDeclaration
	}

	public abstract class Message : ITypedBinarySerializable
	{
		public byte BinaryType => (byte)Type;

		public static Message FromType(Roundchain chaim, MessageType type)
		{
			return type switch
						{
							MessageType.ReleaseDeclaration	=> new ReleaseDeclaration(),
							_								=> throw new IntegrityException("Wrong MessageType"),
						};
		}

		public MessageType Type
		{
			get
			{
				return this switch
							{
								ReleaseDeclaration	=> MessageType.ReleaseDeclaration,
								_					=> throw new IntegrityException("Wrong MessageType"),
							};
			}
		}

		public abstract void Read(BinaryReader r);
		public abstract void Write(BinaryWriter w);
	}

	public class ReleaseDeclaration : Message
	{
		public Account			Signer;
		public ReleaseAddress	Address;
		public string			Stage;			/// stable, beta, nightly, debug,...
		public List<string>		Localizations;
		public byte[]			Signature;

		public bool				Valid => Address.Valid;

		public ReleaseDeclaration()
		{
		}

		public ReleaseDeclaration(PrivateAccount signer, ReleaseAddress address, string stage, List<string> locs, byte[] signature)
		{
			Signer = signer;
			Address = address;
			Stage = stage;
			Localizations = locs;
			Signature = signature;
		}
		
		public void HashWrite(BinaryWriter w)
		{
			w.Write(Signer);
			w.Write(Address);
			w.WriteUtf8(Stage);
			w.Write(Localizations, i => w.WriteUtf8(i));
		}

		public override void Read(BinaryReader r)
		{
			Address = r.ReadReleaseAddress();
			Stage = r.ReadUtf8();
			Localizations = r.ReadList(() => r.ReadUtf8());
			Signature = r.ReadSignature();
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Stage);
			w.Write(Localizations, i => w.WriteUtf8(i));
			w.Write(Signature);
		}
	}

	//public class ReleaseRequest : IBinarySerializable
	//{
	//	public ReleaseAddress	Address;
	//	public string			Localization; /// empty means default
	//
	//	public bool				Valid => Address.Valid;
	//
	//	public ReleaseRequest()
	//	{
	//	}
	//
	//	public ReleaseRequest(ReleaseAddress address, string localization)
	//	{
	//		Address = address;
	//		Localization = localization;
	//	}
	//
	//	public void Read(BinaryReader r)
	//	{
	//		Address = r.ReadReleaseAddress();
	//		Localization = r.ReadUtf8();
	//	}
	//
	//	public void Write(BinaryWriter w)
	//	{
	//		w.Write(Address);
	//		w.WriteUtf8(Localization);
	//	}
	//}
}
