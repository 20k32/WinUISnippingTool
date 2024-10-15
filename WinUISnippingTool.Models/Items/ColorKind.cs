namespace WinUISnippingTool.Models.Items;

public sealed class ColorKind
{
    public string Hex { get; private set; }

    public ColorKind(string hex)
    {
        Hex = hex;
    }
}
