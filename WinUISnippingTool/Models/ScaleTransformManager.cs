using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
namespace WinUISnippingTool.Models;

internal sealed class ScaleTransformManager
{
    private Size tempScaleCenterCoords;
    private Size transformObject;
    private Size relativeEntity;
    public ScaleTransform TransfromSource { get; private set; }

    public ScaleTransformManager()
    {
        TransfromSource = new();
    }

    public void SetTransformObject(Size transformObject)
    {
        this.transformObject = transformObject;
    }

    public Size ActualSize => transformObject;

    public void SetRelativeObject(Size relativeEntity)
    {
        this.relativeEntity = relativeEntity;
    }

    public void SetScaleCenterCoords(Size size)
    {
        tempScaleCenterCoords = size;
        TransfromSource.CenterX = size.Width / 2;
        TransfromSource.CenterY = size.Height / 2;
    }


    public void Transform(Size size)
    {
        var scaleX = size.Width / transformObject.Width;
        var scaleY = size.Height / transformObject.Height;

        var scale = Math.Min(scaleX, scaleY);

        TransfromSource.ScaleX = scale;
        TransfromSource.ScaleY = scale;
    }

    public void Transform()
    {
        var scaleX = relativeEntity.Width / transformObject.Width;
        var scaleY = relativeEntity.Height / transformObject.Height;

        var scale = Math.Min(scaleX, scaleY);

        TransfromSource.ScaleX = scale;
        TransfromSource.ScaleY = scale;
    }

    public void ResetTransform()
    {
        TransfromSource.ScaleX = 1;
        TransfromSource.ScaleY = 1;
    }

}
