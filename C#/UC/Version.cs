//using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System;
using UC.Net;

namespace UC
{
	public class Version : IEquatable<Version>, IComparable, IBinarySerializable
	{
		public ushort Era;
		public ushort Generation;
		public ushort Release;
		public ushort Build;

		public string EG => $"{Era}.{Generation}";
		public string EGR => EG + $".{Release}";
		public string EGRB => EGR + $".{Build}";

		public readonly static Version Zero = new Version(0, 0, 0, 0);

		public Version()
		{
		}

		public Version(ushort era, ushort release, ushort revision, ushort build)
		{
			Era = era;
			Generation = release;
			Release = revision;
			Build = build;
		}

		public override string ToString()
		{
			return $"{Era}.{Generation}.{Release}.{Build}";
		}

		public static Version Parse(string s)
		{
			var c = s.Split('.').ToArray();

			var v = new Version();

			v.Era = ushort.Parse(c[0]);
					
			if(c.Length > 1)
				v.Generation = ushort.Parse(c[1]);

			if(c.Length > 2)
				v.Release = ushort.Parse(c[2]);

			if(c.Length > 3)
				v.Build = ushort.Parse(c[3]);

			return v;
		}

		public static Version Read(BinaryReader r)
		{
			return new Version
					{	
						Era = r.ReadUInt16(), 
						Generation = r.ReadUInt16(), 
						Release = r.ReadUInt16(), 
						Build = r.ReadUInt16()
					};
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Version);
		}

		public bool Equals(Version other)
		{
			return other != null &&
				   Era == other.Era &&
				   Generation == other.Generation &&
				   Release == other.Release &&
				   Build == other.Build;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Era, Generation, Release, Build);
		}

		public int CompareTo(object obj)
		{
			var o = obj as Version;

			if(o > this)
				return -1;
			else if(o < this)
				return 1;
			else
				return 0;
		}

		void IBinarySerializable.Read(BinaryReader r)
		{
			Era = r.ReadUInt16();
			Generation = r.ReadUInt16(); 
			Release = r.ReadUInt16();
			Build = r.ReadUInt16();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Era);
			w.Write(Generation);
			w.Write(Release);
			w.Write(Build);
		}

		public static bool operator < (Version a, Version b)
		{
			if(a.Era < b.Era)
				return true;

			if(a.Generation < b.Generation)
				return true;

			if(a.Release < b.Release)
				return true;

			if(a.Build < b.Build)
				return true;

			return false;
		}

		public static bool operator > (Version a, Version b)
		{
			if(a.Era > b.Era)
				return true;

			if(a.Generation > b.Generation)
				return true;

			if(a.Release > b.Release)
				return true;

			if(a.Build > b.Build)
				return true;

			return false;
		}

		public static bool operator == (Version left, Version right)
		{
			return EqualityComparer<Version>.Default.Equals(left, right);
		}

		public static bool operator !=(Version left, Version right)
		{
			return !(left == right);
		}
	}
}
