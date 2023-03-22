namespace UC.Umc.Enums;

public enum AuthorStatus
{
	Auction,
	Watched,
	Owned,
	Free,
	Reserved,
	Hidden
}

public enum BidStatus
{
	None,
	Higher,
	Lower
}

public enum NotificationType
{
    ProductOperations,
	SystemEvent,
	AuthorOperations,
	TokenOperations,
	Server,
	Wallet
}

public enum Severity
{
    High,
	Mid,
	Low
}

public enum TransactionStatus
{
	None,
    Pending,
	Sent,
	Received,
	Failed,
}
