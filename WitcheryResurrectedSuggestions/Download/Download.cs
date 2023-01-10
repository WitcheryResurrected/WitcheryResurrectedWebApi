using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace WitcheryResurrectedSuggestions.Download;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Downloadable : IComparable<Downloadable>
{
    public string Id { get; }

    public string Name { get; }

    public DownloadFile[] Paths { get; }
    public DateTimeOffset Release { get; }
    public Changelog Changelog { get; }

    public Downloadable(string id, string name, IEnumerable<DownloadFile> paths, DateTimeOffset release, Changelog changelog)
    {
        Id = id;
        Name = name;
        Paths = paths.ToArray();
        Release = release;
        Changelog = changelog;
    }

    public int CompareTo(Downloadable? other)
    {
        if (ReferenceEquals(this, other)) return 0;

        return ReferenceEquals(null, other) ? 1 : other.Release.CompareTo(Release);
    }
}

public class Changelog
{
    public List<string> Additions { get; } = new();
    public List<string> Removals { get; } = new();
    public List<string> Changes { get; } = new();

    public Changelog(IEnumerable<string> text)
    {
        foreach (var s in text)
        {
            if (string.IsNullOrWhiteSpace(s)) continue;
            if (s.StartsWith("+")) Additions.Add(s[1..].Trim());
            else if (s.StartsWith("-")) Removals.Add(s[1..].Trim());
            else if (s.StartsWith("*")) Changes.Add(s[1..].Trim());
            else Changes.Add(s.Trim());
        }
    }
}
