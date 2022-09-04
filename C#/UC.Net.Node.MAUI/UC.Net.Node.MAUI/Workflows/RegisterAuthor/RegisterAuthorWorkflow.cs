namespace UC.Net.Node.MAUI.Workflows.RegisterAuthor;

internal class RegisterAuthorWorkflow : IRegisterAuthorWorkflow
{
    public string Name { get; set; }
    public string Title { get; set; }

    public void Initialize()
    {
        Name = string.Empty;
        Title = string.Empty;
    }
}
