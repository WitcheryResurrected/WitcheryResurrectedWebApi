using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WitcheryResurrectedWebApi.Download;

namespace WitcheryResurrectedWebApi.Controllers;

[EnableCors("CorsPolicy")]
public class DownloadsController : Controller
{
    private readonly IAccessTokenManager _accessTokenManager;
    private readonly IDownloadManager _downloadManager;

    public DownloadsController(
        IAccessTokenManager accessTokenManager,
        IDownloadManager downloadManager
    )
    {
        _accessTokenManager = accessTokenManager;
        _downloadManager = downloadManager;
    }

    [HttpGet("downloads")]
    public ActionResult<List<Downloadable>> Downloads([FromQuery] int limit, [FromQuery] string? after)
    {
        var downloads = _downloadManager.Downloads(limit, after);
        return downloads == null ? StatusCode(404) : downloads;
    }

    [HttpGet("download/{name}/{file}")]
    public async Task<IActionResult> Download([FromRoute] string name, [FromRoute] string file)
    {
        var stream = await _downloadManager.Download(name, file);
        return stream == null ? StatusCode(404) : File(stream, "application/java-archive", file);
    }
}
