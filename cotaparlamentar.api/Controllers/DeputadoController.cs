using cotaparlamentar.api.Authorization;
using cotaparlamentar.api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cotaparlamentar.api.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DeputadoController : ControllerBase
{
    private readonly DeputadoService _deputadoService;
    public DeputadoController(DeputadoService deputadoService)
    {
        _deputadoService = deputadoService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var result = _deputadoService.BuscaTodosDeputadosSiteCompletoPorIdPerfil();
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Route("{nuDeputadoId:int}")]
    public IActionResult AtualizacaoDeputadosGet(int nuDeputadoId)
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var result = _deputadoService.BuscaTodosDeputadosSiteCompletoPorIdPerfil(nuDeputadoId);
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    [Route("foto/{nuDeputadoId:int}")]
    public async Task<IActionResult> AtualizacaoFotoDeputado(int nuDeputadoId)
    {
        try
        {
            var watch = Stopwatch.StartNew();
            await _deputadoService.AtualizacaoDeFoto(nuDeputadoId);
            watch.Stop();
            return Ok($"Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    [Route("foto")]
    public async Task<IActionResult> AtualizacaoFotoDeputado()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            await _deputadoService.AtualizacaoDeFoto();
            watch.Stop();
            return Ok($"Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
