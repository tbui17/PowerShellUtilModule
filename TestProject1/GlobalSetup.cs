namespace TestProject1;


[SetUpFixture]
public class GlobalSetup
{

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Utils.CreateTestDataFolder();
    }
    
}