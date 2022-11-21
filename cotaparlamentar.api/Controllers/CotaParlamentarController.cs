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
    public IActionResult Get(string? data)
    {
        try
        {
            var dataP = data ?? DateTime.Now.AddMonths(-1).ToString("MM/yyyy");
            var watch = Stopwatch.StartNew();
            var result = _cotaParlamentarService.BuscarCotaParlamentarPorData(dataP);
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }

    [HttpGet]
    [Route("{nuDeputadoId:int}")]
    public IActionResult GetDeputado(string? data, int nuDeputadoId)
    {
        try
        {
            var dataP = data ?? DateTime.Now.AddMonths(-1).ToString("MM/yyyy");
            var watch = Stopwatch.StartNew();
            var result = _cotaParlamentarService.BuscarCotaParlamentarPorDataId(dataP, nuDeputadoId);
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }
}
