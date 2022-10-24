namespace UC.Net.Node.MAUI.Models;

public partial class Wallet : ObservableObject
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Unts { get; set; }
    public string IconCode { get; set; }

    public bool IsSelected { get; set; }

	// lets say 1 unts = $1 unless we can recieve rate
	public string DisplayAmount => $"{Unts} UNT (${Unts})";

    public Color AccountColor { get; set; }
}
