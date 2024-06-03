namespace PowerShellStandardModule1.Models;

public abstract class AbstractNode<T>
{
    public int Height { get; set; }
    public int Index { get; set; }
    public required T Value { get; set; }
}