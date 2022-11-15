using cotaparlamentar.api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cotaparlamentar.api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CotaParlamentarController : ControllerBase
{
    private readonly CotaParlamentarService _cotaParlamentarService;
    public CotaParlamentarController(CotaParlamentarService cotaParlamentarService)
    {
        _cotaParlamentarService = cotaParlamentarService;
    }

    [HttpGet]
    [Route("{data}")]
    public IActionResult Get(string data)
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var result = _cotaParlamentarService.BuscarCotaParlamentarPorData(data);
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }

    [HttpGet]
    [Route("{data}/{nuDeputadoId:int}")]
    public IActionResult GetDeputado(string data, int nuDeputadoId)
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var result = _cotaParlamentarService.BuscarCotaParlamentarPorDataId(data, nuDeputadoId);
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }
}
