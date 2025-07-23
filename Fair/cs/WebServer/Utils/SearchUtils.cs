using NBitcoin.Secp256k1;

namespace Uccs.Fair;

internal class SearchUtils
{
	internal static bool IsMatch(Site site, string? title)
	{
		return string.IsNullOrEmpty(title)
			|| site.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) != -1
			|| site.Nickname.IndexOf(title, StringComparison.OrdinalIgnoreCase) != -1;
	}

	internal static bool IsMatch(Proposal proposal, string? search)
	{
		if (string.IsNullOrEmpty(search)) {
			return true;
		}

		string id = proposal.Id.ToString();
		if (id == search)
		{
			return true;
		}

		string flags = proposal.Flags.ToString();
		if (flags.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1)
		{
			return true;
		}

		if (proposal.Text?.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1)
		{
			return true;
		}

		return false;
	}

	internal static bool IsMatch(Review review, string? search)
	{
		if (string.IsNullOrEmpty(search))
		{
			return true;
		}

		string id = review.Id.ToString();
		if (id == search)
		{
			return true;
		}

		string publication = review.Publication.ToString();
		if (publication == search)
		{
			return true;
		}

		string creator = review.Creator.ToString();
		if (creator == search)
		{
			return true;
		}

		string status = review.Status.ToString();
		if (status == search)
		{
			return true;
		}

		if (review.Text?.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1)
		{
			return true;
		}

		if (review.TextNew?.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1)
		{
			return true;
		}

		string rating = review.Rating.ToString();
		if ( rating == search)
		{
			return true;
		}

		return false;
	}

	internal static bool IsMatch(Publication publication, string? search)
	{
		if (string.IsNullOrEmpty(search))
		{
			return true;
		}

		string id = publication.Id.ToString();
		if (id == search)
		{
			return true;
		}

		string category = publication.Category.ToString();
		if (category == search)
		{
			return true;
		}

		//string creator = publication.Creator.ToString();
		//if (creator == search)
		//{
		//	return true;
		//}

		string product = publication.Product.ToString();
		if (product == search)
		{
			return true;
		}

		return false;
	}

	internal static bool IsMatch(Publication publication, Product product, string query)
	{
		string productTitle = PublicationUtils.GetTitle(publication, product);
		if(string.IsNullOrEmpty(productTitle))
		{
			return false;
		}

		return productTitle.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1;
	}
}
