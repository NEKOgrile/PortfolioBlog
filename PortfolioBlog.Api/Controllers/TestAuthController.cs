using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioBlog.Api.Controllers;

[ApiController]
[Route("test")]
public class TestAuthController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly() => Ok("OK Admin");

    [HttpGet("author")]
    [Authorize(Roles = "Admin,Author")]
    public IActionResult AuthorOrAdmin() => Ok("OK Author/Admin");
}
