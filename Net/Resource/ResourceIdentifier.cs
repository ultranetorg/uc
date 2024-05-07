using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class ResourceIdentifier
	{
		public Ura			Addres { get; set; }
		public ResourceId	Id { get; set; }

		public static implicit operator Ura(ResourceIdentifier d) => d.Addres;
		public static implicit operator ResourceId(ResourceIdentifier d) => d.Id;

		public ResourceIdentifier()
		{
		}

		public ResourceIdentifier(Ura addres)
		{
			Addres = addres;
		}

		public ResourceIdentifier(ResourceId id)
		{
			Id = id;
		}
	}
}
