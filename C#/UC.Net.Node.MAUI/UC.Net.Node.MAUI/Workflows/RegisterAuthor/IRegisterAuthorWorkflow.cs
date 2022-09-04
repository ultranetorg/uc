namespace UC.Net.Node.MAUI.Workflows.RegisterAuthor;

internal interface IRegisterAuthorWorkflow : IWorkflow
{
    string Name { get; set; }
    string Title { get; set; }
}
