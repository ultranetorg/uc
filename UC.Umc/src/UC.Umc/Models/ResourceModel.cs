namespace UC.Umc.Models;

public class ResourceModel
{
	protected UC.DomainModels.ResourceModel _entry;

	public int			Id { get; protected set; }
	public string		Name { get; set; } // _entry.Title
	public string?		Owner { get; internal set; }
	public string?		Version { get; internal set; }
	public Color?		Color { get; internal set; }

	public DomainModel Author { get; set; }

	public char? Abbr => Name?.FirstOrDefault();

	public ResourceModel()
	{
	}

	public ResourceModel(UC.DomainModels.ResourceModel entry)
	{
		_entry = entry;
	}

	public ResourceModel(string name, string? owner = null, Color? color = null, string? version = null)
	{
		Name = name;
		Owner = owner;
		Color = color;
		Version = version;
	}
}
