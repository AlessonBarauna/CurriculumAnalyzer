using System.Text.Json;
using CurriculumAnalyzer.API.Data;
using CurriculumAnalyzer.API.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CurriculumAnalyzer.API.Controllers;

[ApiController]
[Route("api/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AnalysisController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var history = await _dbContext.Analyses
            .Include(a => a.Curriculum)
            .OrderByDescending(a => a.AnalysisDate)
            .Select(a => new
            {
                a.Id,
                a.OverallScore,
                a.AnalysisDate,
                a.Curriculum.FileName,
                a.Curriculum.ExperienceLevel,
                a.Curriculum.Specialization,
                a.Curriculum.MarketObjective,
                SalaryMin = a.EstimatedMinSalary,
                SalaryMax = a.EstimatedMaxSalary
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnalysis(string id)
    {
        var analysis = await _dbContext.Analyses
            .Include(a => a.Curriculum)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (analysis == null) return NotFound();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var dto = new AnalysisResponseDto
        {
            Id = analysis.Id,
            CurriculumId = analysis.CurriculumId,
            AnalysisDate = analysis.AnalysisDate,
            OverallScore = analysis.OverallScore,
            ScoreExplanation = analysis.ScoreExplanation,
            EstimatedSalaryRange = new SalaryRangeDto
            {
                Min = analysis.EstimatedMinSalary,
                Max = analysis.EstimatedMaxSalary,
                Currency = "BRL"
            },
            Sections = TryDeserialize<Dictionary<string, SectionScoreDto>>(analysis.SectionsJson, options) ?? new(),
            Strengths = TryDeserialize<List<FeedbackDto>>(analysis.StrengthsJson, options) ?? new(),
            Weaknesses = TryDeserialize<List<FeedbackDto>>(analysis.WeaknessesJson, options) ?? new(),
            Opportunities = TryDeserialize<List<OpportunityDto>>(analysis.OpportunitiesJson, options) ?? new(),
            ActionPlan = TryDeserialize<List<ActionItemDto>>(analysis.ActionPlanJson, options) ?? new(),
            JobRecommendations = TryDeserialize<List<JobRecommendationDto>>(analysis.JobRecommendationsJson, options) ?? new()
        };

        return Ok(dto);
    }

    private static T? TryDeserialize<T>(string json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json, options); }
        catch { return default; }
    }
}
