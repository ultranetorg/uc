using Uuc.Pages;

namespace Uuc.Common.Configurations;

public static class PagesConfiguration
{
	public static MauiAppBuilder ConfigurePages(this MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<AppShell>();

		Type[] pageAndPageModels = GetPagesAndPageModelsTypes();
		Type[] pagesTypes = GetPagesTypes(pageAndPageModels);
		RegisterPages(builder, pagesTypes);
		RegisterPageModels(builder, pageAndPageModels, pagesTypes);

		return builder;
	}

	private static Type[] GetPagesAndPageModelsTypes()
	{
		string? typeNamespace = typeof(_).Namespace;
		IEnumerable<Type> types = typeof(_).Assembly.GetTypes()
			.Where(x => x.Namespace != null && x.Namespace.StartsWith(typeNamespace!) &&
						!x.IsAbstract &&
						(x.Name.EndsWith("Page") || x.Name.EndsWith("PageModel")));
		return types.ToArray();
	}

	private static Type[] GetPagesTypes(Type[] pageAndPageModels)
	{
		return pageAndPageModels.Where(x => x.Name.EndsWith("Page") && x.IsSubclassOf(typeof(ContentPage))).ToArray();
	}

	private static void RegisterPages(MauiAppBuilder builder, Type[] pagesTypes)
	{
		foreach (Type type in pagesTypes)
		{
			builder.Services.AddTransient(type);
		}
	}

	private static void RegisterPageModels(MauiAppBuilder builder, Type[] pageAndPageModels, Type[] pagesTypes)
	{
		var pageModelsTypes = pageAndPageModels.Where(x => x.Name.EndsWith("Model")).ToArray();
		foreach (Type type in pageModelsTypes)
		{
			var correspondedPageTypeName = type.Name.Substring(0, type.Name.Length - 5);
			if (pagesTypes.Any(x => x.Namespace == type.Namespace && x.Name == correspondedPageTypeName))
			{
				builder.Services.AddTransient(type);
			}
		}
	}
}
