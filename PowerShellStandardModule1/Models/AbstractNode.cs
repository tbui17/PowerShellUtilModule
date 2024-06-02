namespace PowerShellStandardModule1.Models;

public abstract record AbstractNode<T>
{
    public int Height;
    public int Index;
    public required T Value;
}