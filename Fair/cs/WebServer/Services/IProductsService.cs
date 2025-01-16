using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IProductsService
{
	ProductEntry GetProduct([NotEmpty] string productId);
}
