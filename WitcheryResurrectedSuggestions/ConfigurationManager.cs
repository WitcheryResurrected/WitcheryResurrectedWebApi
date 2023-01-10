using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WitcheryResurrectedSuggestions;

public class ProgramConfiguration
{
    public string? Webhook { get; set; }

    public string? BotToken { get; set; }

    public string? GuildId { get; set; }

    public string? SuggestionsChannel { get; set; }
}

public interface IConfigurationManager : IHostedService, IDisposable
{
    ProgramConfiguration Config { get; }
    ISet<string> AccessTokens { get; }

    Task<string> HashPassword(string text);

    async Task<bool> IsAuthenticated(string? password) => password != null && AccessTokens.Contains(await HashPassword(password));
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ConfigurationManager : IConfigurationManager
{
    public ProgramConfiguration Config { get; private set; } = new();
    public ISet<string> AccessTokens { get; } = new HashSet<string>();

    private HashAlgorithm _hasher = SHA512.Create();
    private readonly string _configFile;
    private readonly string _accessTokensFile;

    public ConfigurationManager(string configFile, string accessTokensFile)
    {
        _configFile = configFile;
        _accessTokensFile = accessTokensFile;
    }

    public async Task<string> HashPassword(string text)
    {
        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(text);
        await writer.FlushAsync();
        stream.Position = 0;
        return BitConverter.ToString(await _hasher.ComputeHashAsync(stream));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (File.Exists(_configFile))
        {
            await using var configStream = File.OpenRead(_configFile);
            var config = await JsonSerializer.DeserializeAsync<ProgramConfiguration>(
                configStream,
                cancellationToken: cancellationToken
            );

            if (config != null) Config = config;
        }

        if (File.Exists(_accessTokensFile))
        {
            await using var tokenStream = File.OpenRead(_accessTokensFile);
            using var reader = new BinaryReader(tokenStream);
            var hasher = HashAlgorithm.Create(reader.ReadString());
            if (hasher != null) _hasher = hasher;
            var count = reader.ReadInt32();
            var buffer = new byte[64];
            for (var i = 0; i < count; ++i)
            {
                reader.Read(buffer);
                AccessTokens.Add(BitConverter.ToString(buffer));
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose()
    {
        _hasher.Dispose();
        GC.SuppressFinalize(this);
    }
}
