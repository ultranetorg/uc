using UC.Umc.Models;
using UC.Umc.Models.Common;

namespace UC.Umc.Services;

public interface IServicesMockData
{
	public IList<AccountModel> Accounts { get; }
	public IList<DomainModel> Domains { get; }
	public IList<ResourceModel> Resources { get; }
	public IList<TransactionModel> Transactions { get; }
	public IList<AccountColor> AccountColors { get; }
	public IList<Emission> Emissions { get; }
	public IList<Notification> Notifications { get; }
	public IList<Bid> BidsHistory { get; }
	public IList<string> HelpQuestions { get; }
}
