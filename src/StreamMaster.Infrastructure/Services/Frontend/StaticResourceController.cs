using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using StreamMaster.Infrastructure.Extensions;
using StreamMaster.Infrastructure.Services.Frontend.Mappers;

namespace StreamMaster.Infrastructure.Services.Frontend
{
    [Authorize(Policy = "UI")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class StaticResourceController(IEnumerable<IMapHttpRequestsToDisk> requestMappers, ILogger<StaticResourceController> logger) : Controller
    {
        [HttpGet("")]
        [HttpGet("/{**path:regex(^(?!(m|V|s|api|feed)/).*)}")]
        public async Task<IActionResult> Index([FromRoute] string path)
        {
            return await MapResource(path);
        }

        [EnableCors("AllowGet")]
        [AllowAnonymous]
        [HttpGet("/content/{**path:regex(^(?!api/).*)}")]
        public async Task<IActionResult> IndexContent([FromRoute] string path)
        {
            return await MapResource("Content/" + path);
        }

        [HttpGet("/swagger/{**path:regex(^(?!api/).*)}")]
        public async Task<IActionResult> IndexSwagger([FromRoute] string path)
        {
            return await MapResource("swagger/" + path);
        }

        [EnableCors("AllowGet")]
        [AllowAnonymous]
        [HttpGet("/images/{**path:regex(^(?!api/).*)}")]
        public async Task<IActionResult> IndexImages([FromRoute] string path)
        {
            return await MapResource("images/" + path);
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<IActionResult> LoginPage()
        {
            return await MapResource("login");
        }

        private async Task<IActionResult> MapResource(string path)
        {
            path = "/" + (path ?? "");

            IMapHttpRequestsToDisk? mapper = requestMappers.SingleOrDefault(m => m.CanHandle(path));

            if (mapper != null)
            {
                IActionResult? result = await mapper.GetResponseAsync(path);

                if (result != null)
                {
                    if (result is FileResult { ContentType: "text/html" })
                    {
                        Response.Headers.DisableCache();
                    }

                    return result;
                }

                return NotFound();
            }

            logger.LogWarning("Couldn't find handler for {path}", path);

            return NotFound();
        }
    }
}
