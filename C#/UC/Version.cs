//using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace UC
{
	public class Version
	{
		public ushort Era;
		public ushort Generation;
		public ushort Release;
		public ushort Build;

		public string EG => $"{Era}.{Generation}";
		public string EGR => EG + $".{Release}";
		public string EGRB => EGR + $".{Build}";

		public bool Valid => true;

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
			var v = s.Split('.').Select(i => ushort.Parse(i)).ToArray();
			return new Version{ Era = v[0], Generation = v[1], Release = v[2], Build = v[3] };
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
	}
}
