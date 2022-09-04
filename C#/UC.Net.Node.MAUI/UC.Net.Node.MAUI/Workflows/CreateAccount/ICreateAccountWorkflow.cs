namespace UC.Net.Node.MAUI.Workflows.CreateAccount;

public interface ICreateAccountWorkflow : IWorkflow
{
    string Password { get; set; }
    string PasswordConfirm { get; set; }
    string Name { get; set; }
    GradientColor Color { get; set; }
}
