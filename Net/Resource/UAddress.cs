using System;
using System.IO;

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

	public class UAddress : IBinarySerializable, IEquatable<UAddress> 
	{
		public string			Scheme { get; set; }
		public string			Zone { get; set; }
		public string			Entity{ get; set; }

		public bool				Valid => !string.IsNullOrWhiteSpace(Scheme) && !string.IsNullOrWhiteSpace(Entity);

		public UAddress()
		{
		}

		public UAddress(string scheme, string zone, string entity)
		{
			Scheme = scheme;
			Zone = zone;
			Entity = entity;
		}

		public override string ToString()
		{
			return ToString(Scheme, Zone, Entity);
		}

		public static string ToString(string scheme, string zone, string entity)
		{
			return (scheme == null ? null : (scheme + ':')) + (zone == null ? $"{entity}" : $"{zone}:{entity}");
		}

		public static UAddress Parse(string v)
		{
			Parse(v, out var s, out var z, out var e);

			return new UAddress(s, z, e);
		}

		public static void Parse(string v, out string protocol, out string zone, out string other)
		{
			int i;
			
			var a = v.IndexOfAny([':', '/'], 0);
			var b = v.IndexOfAny([':', '/'], a + 1);

			if(a != -1 && v[a] == ':')
				protocol = v.Substring(0, a);
			else
			{
				zone = null;
				protocol = null;
				other = v;
				return;
			}
				
			if(a != -1 && b != -1 && v[a] == ':' && v[b] == ':')
			{
				zone = v.Substring(a + 1, b - a - 1);
				i = b + 1; 
			}
			else
			{
				zone = null;
				i = a + 1;
			}

			other = v.Substring(i);
		}

		public override bool Equals(object o)
		{
			return o is UAddress a && Equals(a);
		}

		public bool Equals(UAddress o)
		{
			return Scheme == o.Scheme && Zone == o.Zone && Entity == o.Entity;
		}

 		public override int GetHashCode()
 		{
 			return Entity.GetHashCode();
 		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as UAddress);
		}

		public int CompareTo(UAddress other)
		{
			if(Scheme.CompareTo(other.Scheme) != 0)
				return Scheme.CompareTo(other.Scheme);

			if(Zone != other.Zone)
				return Zone.CompareTo(other.Zone);

			if(Entity.CompareTo(other.Entity) != 0)
				return Entity.CompareTo(other.Entity);

			return 0;
		}

		public static bool operator == (UAddress a, UAddress b)
		{
			return a is null && b is null || a is not null && a.Equals(b);
		}

		public static bool operator != (UAddress left, UAddress right)
		{
			return !(left == right);
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
