namespace UO.Mobile.UUC.Workflows.RestoreAccount;

internal class RestoreAccountWorkflow : IRestoreAccountWorkflow
{
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public string WalletFile { get; set; }

    public void Initialize()
    {
        PublicKey = string.Empty;
        PrivateKey = string.Empty;
        WalletFile = string.Empty;
    }
}
