using Autofac;

namespace UO.Mobile.UUC;

internal static class DependencyInjection
{
    private static readonly Autofac.IContainer _container = BuildContainer();

    private static Assembly ExecutingAssembly => Assembly.GetExecutingAssembly();

    public static T Resolve<T>() => _container.Resolve<T>();

    public static object Resolve(Type type) => _container.Resolve(type);

    private static Autofac.IContainer BuildContainer()
    {
        ContainerBuilder builder = new();

        RegisterViewModels(builder);
        RegisterServices(builder);
        RegisterMockServices(builder);
        RegisterWorkflows(builder);
        RegisterPages(builder);
        RegisterMockData(builder);

        return builder.Build();
    }

    private static void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ExecutingAssembly)
            .Where(x => x.IsInNamespace(typeof(ViewModels._).Namespace) && x.IsClass &&
                        x.Name.EndsWith("ViewModel"));
    }

    private static void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ExecutingAssembly)
            .Where(x => x.IsInNamespace(typeof(Services._).Namespace) && x.IsClass &&
                        x.Name.EndsWith("Service") && !x.Name.EndsWith("MockService"))
            .AsImplementedInterfaces()
            .SingleInstance();
    }

    private static void RegisterMockServices(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ExecutingAssembly)
            .Where(x => x.IsInNamespace(typeof(Services._).Namespace) && x.IsClass &&
                        x.Name.EndsWith("MockService"))
            .AsImplementedInterfaces()
            .SingleInstance();
    }

    private static void RegisterWorkflows(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ExecutingAssembly)
            .Where(x => x.IsInNamespace(typeof(Workflows._).Namespace) && x.IsClass &&
                        x.Name.EndsWith("Workflow"))
            .AsImplementedInterfaces()
            .SingleInstance();
    }

    private static void RegisterPages(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ExecutingAssembly)
            .Where(x => x.IsInNamespace(typeof(Pages._).Namespace) && x.IsClass && x.Name.EndsWith("Page"));
    }

    private static void RegisterMockData(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ExecutingAssembly)
            .Where(x => x.IsInNamespace(typeof(Services._).Namespace) && x.IsClass && x.Name.EndsWith("MockData"))
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}
