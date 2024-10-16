using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.Models;

public static class CoreConstants
{
    public const int MaxLargeWidth = 1154;
    public const int MaxMediumWidth = 954;
    public const int MaxSmallWidth = 754;

    public const int DefaultBitrate = 10_000_000;
    public const int DefaultFramerate = 60;

    public const int MinVideoPlayerWidth = 600;
    public const int MinVideoPlayerHeight = 600;

    public const string DefaultLocalizationBcp = "uk-UA";

    public const double MinScaleCoeff = 0.1;
    public const double MaxScaleCoeff = 2;
    public const double ScaleFactor = 1;
    public const double ScaleStep = MinScaleCoeff;

    public const double BottomPanelHeight = 32;
    public const double MarginLeftRight = 32;
    public const double MarginTopBottom = MarginLeftRight + BottomPanelHeight;
}
