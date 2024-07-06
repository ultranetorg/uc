namespace Uccs.Net
{
	public interface IPasswordAsker
	{
		string			Password { get; }
		bool			Ask(string info);
		void			Create(string[] warning);
		void			ShowError(string message);						
	}

	public class SilentPasswordAsker : IPasswordAsker
	{
		public string		Password { get; set; }

		public SilentPasswordAsker(string password)
		{
			Password = password;
		}

		public bool Ask(string info)
		{
			return true;
		}

		public void Create(string[] warning)
		{
		}

		public void ShowError(string message)
		{
		}
	}
}
