namespace WinUISnippingTool.Models.Items;

internal sealed class ColorKind
{
    public string Hex { get; private set; }

    public ColorKind(string hex)
    {
        Hex = hex;
    }
}
