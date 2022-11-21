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
    }
}
