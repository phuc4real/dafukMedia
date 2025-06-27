using Microsoft.AspNetCore.Mvc;

namespace dafukMedia.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProviderController : ControllerBase
{

    [HttpGet()]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetProviders()
    {
        var providers = new List<string>
        {
            "Provider1",
            "Provider2",
            "Provider3"
        };
        return Ok(providers);
    }
}
