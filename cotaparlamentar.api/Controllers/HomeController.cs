using cotaparlamentar.api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cotaparlamentar.api.Controllers
{
    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public void GetIndex()
        {
            Response.Redirect("swagger/");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public IActionResult GetToken([FromServices] TokenService service, string pass)
        {
            if (!(pass == "moovo"))
                return BadRequest();


            return Ok(service.GenerateToken());
        }
    }
}
