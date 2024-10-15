using Windows.Storage;

namespace WinUISnippingTool.Models.PageParameters;

public sealed class SettingsPageParameter
{
    public readonly string BcpTag;
    public readonly StorageFolder SaveImageLocation;
    public readonly StorageFolder SaveVideoLocation;

    public SettingsPageParameter(string bcpTag, StorageFolder saveImageLocation, StorageFolder saveVideoLocation)
        => (BcpTag, SaveImageLocation, SaveVideoLocation) = (bcpTag, saveImageLocation, saveVideoLocation);
}
