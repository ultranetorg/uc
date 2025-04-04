﻿namespace UC.Umc.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Body { get; set; }
    public Severity Severity { get; set; }
    public NotificationType Type { get; set; }
}

public enum NotificationType
{
    ProductOperations, SystemEvent, AuthorOperations, TokenOperations, Server, Wallet
}

public enum Severity
{
    High, Mid, Low
}
