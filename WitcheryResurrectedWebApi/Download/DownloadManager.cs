using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WitcheryResurrectedWebApi.Download;

public interface IDownloadManager : IHostedService
{
    public Task<(Downloadable downloadable, string directoryName)> AddDownloads(
        string name,
        string? changelog,
        IEnumerable<(DownloadFile File, Func<Stream, Task> CopyTo)> downloadFiles
    );

    public List<Downloadable>? Downloads(int limit, string? after);
    public Task<FileStream?> Download(string name, string file);
}

public class DownloadManager : IDownloadManager
{
    private readonly List<Downloadable> _downloads = new();
    private readonly Dictionary<string, int> _byPath = new();

    private readonly string _directory;

    public DownloadManager(string directory) => _directory = directory;

    private static async ValueTask<DownloadFile[]> ParseDownloadFiles(string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return Array.Empty<DownloadFile>();
        return await JsonSerializer.DeserializeAsync<DownloadFile[]>(
            File.OpenRead(path),
            cancellationToken: cancellationToken
        ) ?? Array.Empty<DownloadFile>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_directory)) return;
        foreach (var directory in Directory.EnumerateDirectories(_directory))
        {
            var path = Path.GetRelativePath(_directory, directory);
            var name = await File.ReadAllTextAsync(Path.Combine(directory, "name.txt"), cancellationToken);
            var date = await File.ReadAllBytesAsync(Path.Combine(directory, "release"), cancellationToken);
            var changes = await File.ReadAllLinesAsync(Path.Combine(directory, "changelog.txt"), cancellationToken);
            var release = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt64(date));

            var download = new Downloadable(
                path,
                name.Replace("\n", "").Trim(),
                await ParseDownloadFiles(Path.Combine(directory, "indices.json"), cancellationToken),
                release,
                new Changelog(changes)
            );

            _downloads.Add(download);
        }

        _downloads.Sort();

        for (var i = 0; i < _downloads.Count; i++) _byPath[_downloads[i].Id] = i;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task<(Downloadable downloadable, string directoryName)> AddDownloads(string name, string? changelog,
        IEnumerable<(DownloadFile File, Func<Stream, Task> CopyTo)> downloadFiles)
    {
        var directoryName = Regex.Replace(name, "[^a-zA-Z0-9_.-]", "");
        var folder = Path.Combine(_directory, directoryName);
        Directory.CreateDirectory(_directory);
        Directory.CreateDirectory(folder);

        var downloadsArray = downloadFiles.ToArray();

        var releaseDate = DateTimeOffset.Now;
        var downloadable = new Downloadable(
            directoryName,
            name,
            downloadsArray.Select(file => file.Item1),
            releaseDate,
            new Changelog(changelog?.Split('\n') ?? Enumerable.Empty<string>())
        );

        await using (var release = File.Create(Path.Combine(folder, "release")))
            await release.WriteAsync(BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeSeconds()));

        await using (var text = File.Create(Path.Combine(folder, "indices.json")))
        {
            await text.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(downloadable.Paths));

            foreach (var file in downloadsArray)
            {
                await using var stream = new FileStream(Path.Combine(folder, file.File.Name), FileMode.Create);

                await file.CopyTo(stream);
            }
        }

        await using (var text = File.CreateText(Path.Combine(folder, "changelog.txt")))
            await text.WriteLineAsync(changelog);

        await using (var text = File.CreateText(Path.Combine(folder, "name.txt")))
            await text.WriteLineAsync(name);

        _downloads.Insert(0, downloadable);
        foreach (var key in _byPath.Keys) ++_byPath[key];
        _byPath[directoryName] = 0;

        return (downloadable, directoryName);
    }

    public List<Downloadable>? Downloads(int limit, string? after)
    {
        int index;
        if (after == null)
        {
            index = 0;
        }
        else
        {
            if (!_byPath.ContainsKey(after)) return null;
            index = _byPath[after] + 1;
            if (index >= _downloads.Count) return new List<Downloadable>();
        }

        var result = new List<Downloadable>(limit);
        for (var i = index; i < Math.Min(index + limit, _downloads.Count - 1); ++i) result.Add(_downloads[i]);

        return result;
    }

    public async Task<FileStream?> Download(string name, string file)
    {
        if (!_byPath.ContainsKey(name))
            return null;

        var download = _downloads[_byPath[name]];

        var index = -1;
        for (var i = 0; i < download.Paths.Length; ++i)
        {
            var downloadPath = download.Paths[i];
            if (downloadPath.Name != file) continue;
            index = i;
            break;
        }

        if (index == -1)
            return null;

        lock (download) ++download.Paths[index].DownloadCount;
        var indices = Path.Combine(_directory, name, "indices.json");

        var downloadFiles = JsonSerializer.Deserialize<DownloadFile[]>(
            await File.ReadAllTextAsync(indices));

        lock (this)
        {
            downloadFiles![index].DownloadCount = download.Paths[index].DownloadCount;

            File.WriteAllText(indices, JsonSerializer.Serialize(downloadFiles));
        }

        return new FileStream(Path.Combine(_directory, name, file), FileMode.Open);
    }
}
