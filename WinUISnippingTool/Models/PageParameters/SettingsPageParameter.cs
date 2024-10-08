using Windows.Storage;

namespace WinUISnippingTool.Models.PageParameters;

internal sealed class SettingsPageParameter
{
    public readonly string BcpTag;
    public readonly StorageFolder SaveImageLocation;

    public SettingsPageParameter(string bcpTag, StorageFolder folder)
        => (BcpTag, SaveImageLocation) = (bcpTag, folder);
}
