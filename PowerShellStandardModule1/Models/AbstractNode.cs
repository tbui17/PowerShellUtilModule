namespace PowerShellStandardModule1.Models;

public abstract record AbstractNode<T>
{
    public required T Value;
    public int Height;
    public int Index;
}