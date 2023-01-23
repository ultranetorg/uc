using UC.Net;

namespace UC.Umc.Models;

public class ProductViewModel
{
	protected ProductEntry _entry;

    public int			Id { get; protected set; }
    public string		Name { get; set; } // _entry.Title
    public string		Owner { get; internal set; }
    public Color		Color { get; internal set; }

    public AuthorViewModel Author { get; set; }

	public char? Abbr => Name?.FirstOrDefault();

	public ProductViewModel()
	{
	}

	public ProductViewModel(ProductEntry entry)
	{
		_entry = entry;
	}

	public ProductViewModel(string name, string owner = null, Color color = null)
	{
		Name = name;
		Owner = owner;
		Color = color;
	}
}
