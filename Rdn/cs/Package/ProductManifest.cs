using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Rdn
{
	public enum PlatformExpressionOperator
	{
		None, Greater, GreaterOrEqual, Less, LessOrEqual, Equal
	}

	public class PlatformExpression : IBinarySerializable
	{
		public PlatformExpressionOperator	Operator;
		public Platform						X;
		public Platform						Y;

		public void Write(BinaryWriter writer)
		{
			throw new NotImplementedException();
		}

		public void Read(BinaryReader reader)
		{
			throw new NotImplementedException();
		}
	}

	public class ProductManifest : IBinarySerializable
	{
		public PlatformExpression[]		Requirements;

		public void Read(BinaryReader reader)
		{
		}

		public void Write(BinaryWriter writer)
		{
		}
	}
}
