using System.Diagnostics.CodeAnalysis;

namespace Uccs.Smp;

public interface IProductsService
{
	ProductEntry GetProduct([NotEmpty] string productId);
}
