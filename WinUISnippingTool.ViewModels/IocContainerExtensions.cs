using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUISnippingTool.ViewModels.Resources;

namespace WinUISnippingTool.ViewModels;

public static class IocContainerExtensions
{
    public static IServiceCollection RegisterInnerWindows(this IServiceCollection collection)
        => collection.AddTransient<SnipScreenWindow>()
                     .AddTransient<VideoCaptureWindow>();

    public static IServiceCollection RegisterViewModels(this IServiceCollection collection)
        => collection.AddSingleton<SnipScreenWindowViewModel>()
                     .AddSingleton<VideoCaptureWindowViewModel>()
                     .AddSingleton<MainPageViewModel>();
}
