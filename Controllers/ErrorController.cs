using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{
    /// <summary>بديل Home/Error: يعيد ProblemDetails عند حدوث استثناء غير معالج.</summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult HandleError() => Problem();
    }
}
