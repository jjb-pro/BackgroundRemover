using PaintDotNet;
using System;
using System.IO;
using System.Reflection;

namespace BackgroundRemover;

public class PluginSupportInfo : IPluginSupportInfo
{
    private readonly Assembly assembly = typeof(PluginSupportInfo).Assembly;

#nullable disable
    public string Author => assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

    public string Copyright => assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

    public string DisplayName => assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;

    public Version Version => assembly.GetName().Version;
#nullable enable

    public Uri WebsiteUri => new("https://aka.jjb-pro.com/pdn-bgrem");
}