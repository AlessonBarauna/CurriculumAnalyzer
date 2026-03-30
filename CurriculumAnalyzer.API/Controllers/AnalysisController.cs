using CurriculumAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurriculumAnalyzer.API.Controllers;

[ApiController]
[Route("api/analysis")]
public class AnalysisController(IAnalysisService analysisService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var history = await analysisService.GetHistoryAsync();
        return Ok(history);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnalysis(string id)
    {
        var analysis = await analysisService.GetByIdAsync(id);
        return analysis is null ? NotFound() : Ok(analysis);
    }

    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] string id1, [FromQuery] string id2)
    {
        var result = await analysisService.CompareAsync(id1, id2);
        if (result is null)
            return NotFound(new { error = "Uma ou ambas as análises não foram encontradas." });

        return Ok(new { before = result.Value.Before, after = result.Value.After });
    }
}
