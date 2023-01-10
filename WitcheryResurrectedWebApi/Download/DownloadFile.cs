using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WitcheryResurrectedWebApi.Download;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public struct DownloadFile
{
    public string Name { get; set; }
    public long Size { get; set; }
    public ModLoader Loader { get; set; }
    public string Version { get; set; }
    public List<Dependency> Dependencies { get; set; }
    public uint DownloadCount { get; set; }

    public DownloadFile(string name, long size, uint downloadCount, List<Dependency> dependencies, ModLoader loader, string version)
    {
        Name = name;
        Size = size;
        DownloadCount = downloadCount;
        Dependencies = dependencies;
        Loader = loader;
        Version = version;
    }
}
    
public enum ModLoader : sbyte
{
    Invalid = -1,
    Forge,
    Fabric
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Dependency
{
    public string? Name { get; set; }
    public string? Link { get; set; }
}
