using System;

namespace UC.Net
{
	public class ProductAddress : IEquatable<ProductAddress>
	{
		public string Author { get; }
		public string Product { get; }

		public virtual bool Valid => !string.IsNullOrWhiteSpace(Author)  && !string.IsNullOrWhiteSpace(Product);

		public ProductAddress(string author, string product)
		{
			Author = author;
			Product = product;
		}

		public override string ToString()
		{
			return Author + "/" + Product;
		}

		public override bool Equals(object obj)
		{
			return obj is ProductAddress address && Equals(address);
		}

		public bool Equals(ProductAddress other)
		{
			return Author == other.Author && Product == other.Product;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Author, Product);
		}

		public static ProductAddress Parse(string v)
		{
			var s = v.Split('/');
			return new ProductAddress(s[0], s[1]);
		}

		public static bool operator ==(ProductAddress left, ProductAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ProductAddress left, ProductAddress right)
		{
			return !(left == right);
		}

	}
}
