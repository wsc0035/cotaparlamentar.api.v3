using cotaparlamentar.api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cotaparlamentar.api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssessorParlamentarController : ControllerBase
{
    private readonly AssessorParlamentarService _contextAccessor;
    public AssessorParlamentarController(AssessorParlamentarService contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    [HttpGet]
    public IActionResult Atualizar()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var result = _contextAccessor.AtualizarAssessor();
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
    public IActionResult GetAsessorParlamentar(int nuDeputadoId)
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var result = _contextAccessor.AtualizacaoAssessorID(nuDeputadoId);
            watch.Stop();
            return Ok($"{result} Tempo: {watch.ElapsedMilliseconds / 1000}s");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
