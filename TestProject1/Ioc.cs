using Autofac;

namespace TestProject1;

public static class Container
{
    public static IContainer Provider => _provider ?? throw new InvalidOperationException("Container not initialized.");
    private static IContainer? _provider;


    public static T Resolve<T>() where T : notnull => Provider.Resolve<T>();

    

    public static void Init(Action<ContainerBuilder> initializer, bool force = false)
    {
        switch (force)
        {
            case false when _provider is not null:
                Console.Error.WriteLine("Container already initialized.");
                return;
            case true when _provider is not null:
                Console.WriteLine("Container was already initialized. Forcing reinitialization.");
                break;
        }

        var builder = new ContainerBuilder();
        initializer(builder);

        var provider = builder.Build();
        _provider = provider;
    }

    
}