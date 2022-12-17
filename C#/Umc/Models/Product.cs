namespace UC.Umc.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Owner { get; internal set; }
    public string Abbr { get; internal set; }
    public Color Color { get; internal set; }

    public AuthorViewModel Author { get; set; }

	public Product()
	{
	}

	public Product(string name, string owner = null, string abbr = null, Color color = null)
	{
		Name = name;
		Owner = owner;
		Abbr = abbr;
		Color = color;
	}
}
