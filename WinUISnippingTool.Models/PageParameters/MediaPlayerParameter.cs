using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.Models.PageParameters;

internal class MediaPlayerParameter
{
    public readonly Uri Uri;

    public MediaPlayerParameter(Uri uri) => Uri = uri;
}
