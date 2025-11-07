using Identity.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemTokenController : ControllerBase
    {
        private readonly ISystemTokenService _systemTokenService;

        public SystemTokenController(ISystemTokenService systemTokenService)
        {
            _systemTokenService = systemTokenService;
        }

        [HttpPost]
        public IActionResult GenerateSystemToken([FromHeader(Name = "x-internal-key")] string internalKey)
        {
            var result = _systemTokenService.GenerateSystemToken(internalKey);

            if (result.IsSuccess)
                return Ok(result);  

            return Unauthorized(result);
        }
        
    }
}
