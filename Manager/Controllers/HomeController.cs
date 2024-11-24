using Microsoft.AspNetCore.Mvc;
using RESTFulSense.Controllers;

namespace Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : RESTFulController
    {
        [HttpGet]
        public ActionResult<string> Get() =>
            Ok("Hello Mario.");
    }
}
