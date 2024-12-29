namespace Uccs.Fair;

public class TopicCommand : FairCommand
{
	public const string Keyword = "topic";

	EntityId FirstTopicId => EntityId.Parse(Args[0].Name);

	public TopicCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help()
							{
								Title = "Create",
								Description = "Creates a new topic",
								Syntax = $"{Keyword} c|create catalogue={EID} product={EID} {SignerArg}={AA}",
								Arguments =	[new ("catalogue", "Number of years in [1..10] range"),
											 new ("product", "Number of years in [1..10] range"),
											 new (SignerArg, "Address of account that owns the catalogue")],
								Examples =	[new (null, $"{Keyword} c catalogue={EID.Examples[0]} product={EID.Examples[1]} {SignerArg}=0x0000fffb3f90771533b1739480987cee9f08d754")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new TopicCreation {Catalogue = GetEntityId("catalogue"), Product = GetEntityId("product")};
											}
						},

						new ()
						{
							Names = ["x", "destroy"],

							Help = new Help
							{ 
								Title = "Destroy",
								Description = "Destroys existing topic and all its associated data",
								Syntax = $"{Keyword} x|destroy {EID}",
								Arguments = [new ("<first>", "Id of a topic to delete")],
								Examples = [new (null, $"{Keyword} x {EID.Examples[0]}")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new TopicDeletion {Topic = FirstTopicId};
											}
						},
						new ()
						{
							Names = ["e", "entity"],

							Help = new Help()
							{
								Title = "Entity",
								Description = "Get topic entity information from MCV database",
								Syntax = $"{Keyword} e|entity {EID}",
								Arguments =	[new ("<first>", "Id of a topic to get information about")],
								Examples =[new (null, $"{Keyword} e {EID.Examples[0]}")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new TopicRequest(FirstTopicId));

												Dump(rp.Topic);
					
												return rp.Topic;
											}
						},

						new ()
						{
							Names = ["l", "list"],

							Help = new Help {Title = "List",
											 Description = "Get topics of a specified catalogue",
											 Syntax = $"{Keyword} l|list {EID}",
											 Arguments = [new ("<first>", "Id of a catalog to get topics from")],
											 Examples = [new (null, $"{Keyword} l {EID.Examples[0]}")]},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new CatalogueTopicsRequest(FirstTopicId));

												Dump(rp.Topics.Select(i => Rdc(new TopicRequest(i)).Topic), ["Id", "Catalogue", "Product", "Reviews"], [i => i.Id, i => i.Catalogue, i => i.Product, i => i.Reviews?.Length]);
					
												return rp.Topics;
											}
						},

					];	
	}
}
