using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioBlog.Api.Data;
using PortfolioBlog.Domain.Entities;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PortfolioBlog.Api.Controllers;

[ApiController]
[Route("articles")]
public class ArticlesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ArticlesController(AppDbContext db) => _db = db;

    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok("Articles controller alive");

    static string Slugify(string input)
    {
        input = input.Trim().ToLowerInvariant();
        input = Regex.Replace(input, @"[^a-z0-9\s-]", "");
        input = Regex.Replace(input, @"\s+", "-");
        input = Regex.Replace(input, @"-+", "-");
        return input;
    }

    bool IsAdmin() => User.IsInRole("Admin");
    string? UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // PUBLIC: liste articles publiés
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublished()
    {
        var items = await _db.Articles
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new { a.Id, a.Title, a.Slug, a.CreatedAtUtc })
            .ToListAsync();

        return Ok(items);
    }

    // ADMIN/AUTHOR: liste (admin=tout, author=ses articles)
    [HttpGet]

    [Authorize(Roles = "Admin,Author")]
    public async Task<IActionResult> GetMineOrAll()
    {
        var uid = UserId();
        if (uid is null) return Unauthorized();

        var q = _db.Articles.AsQueryable();
        if (!IsAdmin())
            q = q.Where(a => a.AuthorId == uid);

        var items = await q
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new { a.Id, a.Title, a.Slug, a.IsPublished, a.CreatedAtUtc, a.UpdatedAtUtc })
            .ToListAsync();

        return Ok(items);
    }

    // ADMIN/AUTHOR: créer
    public record ArticleCreateDto(string Title, string Content, bool IsPublished);

    [HttpPost("create")]


    [Authorize(Roles = "Admin,Author")]
    public async Task<IActionResult> Create([FromBody] ArticleCreateDto dto)
    {
        var uid = UserId();
        if (uid is null) return Unauthorized();

        var slug = Slugify(dto.Title);
        if (await _db.Articles.AnyAsync(x => x.Slug == slug))
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

        var a = new Article
        {
            Title = dto.Title,
            Slug = slug,
            Content = dto.Content,
            IsPublished = dto.IsPublished,
            AuthorId = uid,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Articles.Add(a);
        await _db.SaveChangesAsync();

        return Ok(a);
    }
}
