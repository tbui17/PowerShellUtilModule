using Autofac;
using PowerShellStandardModule1.Commands.PrintTree;

namespace TestProject1.PrintTree;

public class ContainerInit
{
    [OneTimeSetUp]
    public void Init()
    {
        Container.Init(Initializer);
    }

    private static void Initializer(ContainerBuilder b)
    {
        var directory =
            Utils.GetSolutionDirectory() ?? throw new InvalidOperationException("Solution directory not found.");

        b.Register<StringValueSelector>(_ => x => x.Value.Name);

        b.RegisterInstance(directory);
        b
           .RegisterType<PrintTreeService>()
           .WithProperty(x => x.StartingDirectory, directory)
           .OnActivated(x => x.Instance.Init())
           .PropertiesAutowired();

        b
           .RegisterType<PrintTreeCommand>()
           .WithProperty(x => x.StartingDirectory, directory.Name)
           .PropertiesAutowired();
        
    }
}