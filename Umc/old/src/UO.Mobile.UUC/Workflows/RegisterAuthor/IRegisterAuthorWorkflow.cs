namespace UO.Mobile.UUC.Workflows.RegisterAuthor;

internal interface IRegisterAuthorWorkflow : IWorkflow
{
    string Name { get; set; }

    string Title { get; set; }
}
