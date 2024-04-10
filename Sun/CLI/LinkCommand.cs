using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class LinkCommand : Command
	{
		public const string Keyword = "link";

		public LinkCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
				case "c" : 
				case "create" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new ResourceLinkCreation(GetResourceAddress("source"), GetResourceAddress("destination"));
				}

				case "r" : 
				case "remove" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new ResourceLinkDeletion(GetResourceAddress("source"), GetResourceAddress("destination"));
				}

				case "lo" :
		   		case "listoutbounds" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Rdc<ResourceByNameResponse>(new ResourceByNameRequest {Name = ResourceAddress.Parse(Args[1].Name)});
					
					Dump(r.Resource.Outbounds.Select(i => new {L = i, R = Rdc<ResourceByIdResponse>(new ResourceByIdRequest {ResourceId = i.Destination}).Resource}),
						 ["#", "Flags", "Destination", "Destination Data"],
						 [i => i.L.Destination, i => i.L.Flags, i => i.R.Address, i => i.R.Data.Interpretation]);

					return r;
				}

				case "li" :
		   		case "listinbounds" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var r = Rdc<ResourceByNameResponse>(new ResourceByNameRequest {Name = ResourceAddress.Parse(Args[1].Name)});
					
					Dump(r.Resource.Inbounds.Select(i => new {L = i, R = Rdc<ResourceByIdResponse>(new ResourceByIdRequest {ResourceId = i}).Resource}),
						 ["#", "Source", "Source Data"],
						 [i => i.L, i => i.R.Address, i => i.R.Data.Interpretation]);

					return r;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
