namespace Uuc.Common.Helpers;

internal static class TypesHelper
{
	public static Type[] GetPageModelTypes()
	{
		string? typeNamespace = typeof(PageModels._).Namespace;
		IEnumerable<Type> types = typeof(PageModels._).Assembly.GetTypes()
			.Where(x => x.Namespace != null && x.Namespace.StartsWith(typeNamespace!) &&
						!x.IsAbstract &&
						x.Name.EndsWith("PageModel"));
		return types.ToArray();

	}

	public static Type[] GetPagesTypes()
	{
		string? typeNamespace = typeof(Pages._).Namespace;
		IEnumerable<Type> types = typeof(Pages._).Assembly.GetTypes()
			.Where(x => x.Namespace != null && x.Namespace.StartsWith(typeNamespace!) &&
						!x.IsAbstract &&
						x.Name.EndsWith("Page") &&
						x.IsSubclassOf(typeof(ContentPage)));
		return types.ToArray();
	}
}
