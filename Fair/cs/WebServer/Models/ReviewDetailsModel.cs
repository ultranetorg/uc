namespace Uccs.Fair;

public class ReviewDetailsModel : ReviewModel
{
	public string AccountAddress { get; set; }

	public ReviewDetailsModel(Review review, Account account) : base(review)
	{
		AccountAddress = account.Address.ToString();
	}
}
