namespace UO.Mobile.UUC.Workflows.RestoreAccount;

internal interface IRestoreAccountWorkflow : IWorkflow
{
    string PublicKey { get; set; }

    string PrivateKey { get; set; }

    string WalletFile { get; set; }
}
