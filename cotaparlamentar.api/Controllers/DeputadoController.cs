using cotaparlamentar.api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cotaparlamentar.api.Controllers;

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
    [Route("{idperfil:int}")]
    public IActionResult AtualizacaoDeputadosGet(int idperfil)
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var result = _deputadoService.BuscaTodosDeputadosSiteCompletoPorIdPerfil(idperfil);
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
