using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WinUISnippingTool.ViewModels;
using WinUISnippingTool.ViewModels.Resources;
using System.Net.WebSockets;
using WinUISnippingTool.Models.VideoCapture;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Core;

public static class IocContainerExtensions
{
    public static IServiceCollection ConfigurePresentationLayer(this IServiceCollection collection)
        =>  collection.RegisterViewModels()
            .RegisterInnerWindows();
}
