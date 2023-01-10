using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WitcheryResurrectedWebApi;

public interface IAccessTokenManager : IHostedService, IDisposable
{
    Task<bool> IsAuthenticated(string? password);
}

public class AccessTokenManager : IAccessTokenManager
{
    private readonly HashSet<string> _accessTokens = new ();
    private readonly string _accessTokensFile;

    private HashAlgorithm _hasher = SHA512.Create();

    public AccessTokenManager(string accessTokensFile) => _accessTokensFile = accessTokensFile;

    public async Task<bool> IsAuthenticated(string? password)
    {
        if (password == null) return false;
        
        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(password);
        await writer.FlushAsync();
        stream.Position = 0;
        return _accessTokens.Contains(BitConverter.ToString(await _hasher.ComputeHashAsync(stream)));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
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
                _accessTokens.Add(BitConverter.ToString(buffer));
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
