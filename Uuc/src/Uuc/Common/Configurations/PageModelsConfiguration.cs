using Uuc.Common.Helpers;
using Uuc.PageModels;

namespace Uuc.Common.Configurations;

public static class PageModelsConfiguration
{
	public static MauiAppBuilder ConfigurePageModels(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient<AppShellModel>();

		Type[] pagesTypes = TypesHelper.GetPagesTypes();
		Type[] pageModelsTypes = TypesHelper.GetPageModelTypes();
		foreach (Type pageModelsType in pageModelsTypes)
		{
			if (pagesTypes.Any(x => x.Namespace != null && x.Namespace.Replace(".Pages", ".PageModels") == pageModelsType.Namespace &&
									x.Name + "Model" == pageModelsType.Name))
			{
				builder.Services.AddTransient(pageModelsType);
			}
		}

		return builder;
	}
}
