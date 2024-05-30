using System.Configuration;

namespace TestProject1;

public static class EnvVarsExtensions
{
    public static string Get(this EnvVars envVar) =>
        Environment.GetEnvironmentVariable(envVar.ToString()) ??
        throw new ConfigurationErrorsException($"{envVar} not set");
}