using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	/// <summary>
	/// 
	/// ultranet:/domain
	/// ultranet:/domain/reso/ur/ce (/domain/ - корневой)
	/// ultranet:#0123456789ABCDEF
	/// ultranet:$domain/product:hhhhhhhhhhhhhhhhhhh:sssssssssssssssssssssssssssssssssssssssssssssss
	/// 
	/// </summary>

	public class Ura : IBinarySerializable, IEquatable<Ura> 
	{
		public string			Domain { get; set; }
		public string			Resource { get; set; }
		public ReleaseAddress	Release { get; set; }

		public bool				Valid => !string.IsNullOrWhiteSpace(Domain) && !string.IsNullOrWhiteSpace(Resource);

		public Ura()
		{
		}

		public override string ToString()
		{
			if(Domain != null)
				if(Resource == null)
					return $"/{Domain}";
				else
					return $"/{Domain}/{Resource}";
			else
				return $"{Release}";
		}

		public override bool Equals(object o)
		{
			return o is Ura a && Equals(a);
		}

		public bool Equals(Ura o)
		{
			return Domain == o.Domain && Resource == o.Resource && Release == o.Release;
		}

 		public override int GetHashCode()
 		{
 			return Domain.GetHashCode();
 		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as Ura);
		}

		public int CompareTo(Ura other)
		{
			if(Domain.CompareTo(other.Domain) != 0)
				return Domain.CompareTo(other.Domain);

			if(Resource.CompareTo(other.Resource) != 0)
				return Resource.CompareTo(other.Resource);

			///return Release.CompareTo(other.Release);
			throw new NotImplementedException();
		}

		public static bool operator == (Ura a, Ura b)
		{
			return a is null && b is null || a is not null && b is not null && a.Equals(b);
		}

		public static bool operator != (Ura left, Ura right)
		{
			return !(left == right);
		}

		public static Ura Parse(string v)
		{
			throw new NotImplementedException();
		}

		public void Write(BinaryWriter w)
		{
			throw new NotImplementedException();
		}

		public void Read(BinaryReader r)
		{
			throw new NotImplementedException();
		}
	}
}
