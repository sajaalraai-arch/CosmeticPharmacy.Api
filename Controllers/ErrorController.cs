using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{

    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult HandleError() => Problem();
    }
}
