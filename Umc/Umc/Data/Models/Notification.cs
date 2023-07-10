namespace UC.Umc.Models;

public partial class Notification : ObservableObject
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string DateString => Date.ToShortDateString();
    public string Body { get; set; }
    public Severity Severity { get; set; }
    public NotificationType Type { get; set; }

	[ObservableProperty]
	protected bool _isRead;
}
