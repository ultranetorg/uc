namespace UC.Net.Node.MAUI.Workflows;

internal interface IRestoreAccountWorkflow : IWorkflow
{
    string PublicKey { get; set; }
    string PrivateKey { get; set; }
    string WalletFile { get; set; }
}
