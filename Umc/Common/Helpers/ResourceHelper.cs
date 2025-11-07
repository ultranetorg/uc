namespace UC.Umc.Common.Helpers;

public static class ResourceHelper
{
	public static string GetDashboardResource(string key)
	{
		return Properties.Dashboard_Strings.ResourceManager.GetString(key);
	}
	
	public static HelpInfo GetHelpInfo(int index)
	{
		return new HelpInfo()
		{
			Question = GetDashboardResource("HelpQuestion_" + index),
			Answer = GetDashboardResource("HelpAnswer_" + index),
			Prompt = string.Empty,
		};
	}
}
