using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CurriculumAnalyzer.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurriculumAnalyzer.API.Controllers;

[ApiController]
[Route("api/analysis")]
[Authorize]
public class AnalysisController(IAnalysisService analysisService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var history = await analysisService.GetHistoryAsync(UserId);
        return Ok(history);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnalysis(string id)
    {
        var analysis = await analysisService.GetByIdAsync(id, UserId);
        return analysis is null ? NotFound() : Ok(analysis);
    }

    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] string id1, [FromQuery] string id2)
    {
        var result = await analysisService.CompareAsync(id1, id2, UserId);
        if (result is null)
            return NotFound(new { error = "Uma ou ambas as análises não foram encontradas." });

        return Ok(new { before = result.Value.Before, after = result.Value.After });
    }
}
