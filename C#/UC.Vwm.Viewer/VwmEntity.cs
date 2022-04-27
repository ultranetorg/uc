using System;
using System.Collections.Generic;
using System.Text;

namespace UC.Vwm.Viewer
{
	public class VwmEntity
	{
		byte		_flags;
		string		_name;
		string		_type;
		byte	[]	_data;

		public byte [] Data
		{
			get { return _data; }
			set { _data = value; }
		}

		public byte Flags
		{
			get { return _flags; }
			set { _flags = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}
	}
}
