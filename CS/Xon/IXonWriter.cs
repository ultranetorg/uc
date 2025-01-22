namespace Uccs;

public interface IXonWriter
{
	public const string BonHeader = "BON02";

	void Start();
	void Write(Xon s);
	void Finish();
	//void Write(IEnumerable<Xon> items);
};
