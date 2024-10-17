namespace UC.Umc.Models;

public class HelpInfo
{
	public int Id { get; set; }
	public string Question { get; set; }
	public string Answer { get; set; }
	public string Prompt { get; set; }

	public List<int> RelatedSources { get; set; }
}
