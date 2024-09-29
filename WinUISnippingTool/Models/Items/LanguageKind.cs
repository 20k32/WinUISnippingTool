namespace WinUISnippingTool.Models.Items
{
    internal sealed class LanguageKind : ModelBase
    {
        public readonly string BcpTag;

        private string displayName;
        public string DisplayName 
        { 
            get => displayName;
            set
            {
                if(displayName != value)
                {
                    displayName = value;
                    OnPropertyChanged();
                }
            }
        }

        public LanguageKind(string displayName, string bcpTag)
            => (DisplayName, BcpTag) = (displayName, bcpTag);
    }
}
