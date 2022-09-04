namespace UC.Net.Node.MAUI.Workflows.CreateAccount;

public class CreateAccountWorkflow : ICreateAccountWorkflow
{
    public string Password { get; set; }
    public string PasswordConfirm { get; set; }
    public string Name { get; set; }
    public GradientColor Color { get; set; }

    public void Initialize()
    {
        Name = default(string);
        Color = default(GradientColor);
    }
}
