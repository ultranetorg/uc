using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class ProductTypeValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(ProductType? productType)
	{
		if (productType != null && productType.Value == ProductType.None)
		{
			throw new InvalidProductTypeException(productType.Value);
		}
	}
}
