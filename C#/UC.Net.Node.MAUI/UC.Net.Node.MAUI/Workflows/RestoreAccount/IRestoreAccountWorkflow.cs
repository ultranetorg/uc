namespace UC.Net.Node.MAUI.Workflows.RestoreAccount;

internal interface IRestoreAccountWorkflow : IWorkflow
{
    string PublicKey { get; set; }
    string PrivateKey { get; set; }
    string WalletFile { get; set; }
}
