using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WitcheryResurrectedSuggestions.Download;

namespace WitcheryResurrectedSuggestions.Controllers;

[EnableCors("CorsPolicy")]
public class DownloadsController : Controller
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IDownloadManager _downloadManager;

    public DownloadsController(
        IConfigurationManager configurationManager,
        IDownloadManager downloadManager
    )
    {
        _configurationManager = configurationManager;
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
