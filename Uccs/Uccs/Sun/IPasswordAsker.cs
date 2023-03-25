using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;
using System;
using System.Numerics;

namespace Uccs.Net
{
	public interface IPasswordAsker
	{
		string			Password { get; }
		bool			Ask(string info);
		void			ShowError(string message);						
	}

	public interface INewPasswordAsker
	{
		bool		Ask(string info);
		string		Password { get; }
	}

	public class FixedPasswordAsker : IPasswordAsker
	{
		public string		Password { get; set; }

		public FixedPasswordAsker(string password)
		{
			Password = password;
		}

		public bool Ask(string info)
		{
			return true;
		}

		public void ShowError(string message){ }
	}
}
