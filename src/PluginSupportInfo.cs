using PaintDotNet;
using System.Reflection;

namespace BackgroundRemover;

public class PluginSupportInfo : IPluginSupportInfo
{
    private readonly Assembly _assembly = typeof(PluginSupportInfo).Assembly;

#nullable disable
    public string Author => _assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

    public string Copyright => _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

    public string DisplayName => _assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;

    public Version Version => _assembly.GetName().Version;
#nullable enable

    public Uri WebsiteUri => new("https://jjb-pro.com/");
}