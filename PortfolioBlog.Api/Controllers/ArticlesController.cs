using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioBlog.Api.Data;
using PortfolioBlog.Api.Models.Articles;
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
        input = (input ?? "").Trim().ToLowerInvariant();
        input = Regex.Replace(input, @"[^a-z0-9\s-]", "");
        input = Regex.Replace(input, @"\s+", "-");
        input = Regex.Replace(input, @"-+", "-");
        return input;
    }

    bool IsAdmin() => User.IsInRole("Admin");
    string? UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    async Task<Article?> FindAccessibleArticle(int id)
    {
        var uid = UserId();
        if (uid is null) return null;

        var q = _db.Articles.AsQueryable();
        if (!IsAdmin())
            q = q.Where(a => a.AuthorId == uid);

        return await q.FirstOrDefaultAsync(a => a.Id == id);
    }

    // PUBLIC: articles publiés
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
    [HttpPost]
    [Authorize(Roles = "Admin,Author")]
    public async Task<IActionResult> Create([FromBody] ArticleCreateDto dto)
    {
        var uid = UserId();
        if (uid is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { error = "Title is required" });

        var slug = Slugify(dto.Title);
        if (await _db.Articles.AnyAsync(x => x.Slug == slug))
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

        var a = new Article
        {
            Title = dto.Title,
            Slug = slug,
            Content = dto.Content ?? "",
            IsPublished = dto.IsPublished,
            AuthorId = uid,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Articles.Add(a);
        await _db.SaveChangesAsync();

        return Ok(new { a.Id, a.Title, a.Slug, a.IsPublished, a.CreatedAtUtc });
    }

    // ADMIN/AUTHOR: update (admin=tout, author=ses articles)
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Author")]
    public async Task<IActionResult> Update(int id, [FromBody] ArticleUpdateDto dto)
    {
        var a = await FindAccessibleArticle(id);
        if (a is null) return NotFound();

        var titleChanged = dto.Title is not null && dto.Title != a.Title;

        if (dto.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { error = "Title cannot be empty" });

            a.Title = dto.Title;
        }

        if (dto.Content is not null)
            a.Content = dto.Content;

        if (dto.IsPublished.HasValue)
            a.IsPublished = dto.IsPublished.Value;

        if (titleChanged)
        {
            var newSlug = Slugify(a.Title);
            if (await _db.Articles.AnyAsync(x => x.Id != a.Id && x.Slug == newSlug))
                newSlug = $"{newSlug}-{Guid.NewGuid().ToString("N")[..6]}";

            a.Slug = newSlug;
        }

        a.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { a.Id, a.Title, a.Slug, a.IsPublished, a.CreatedAtUtc, a.UpdatedAtUtc });
    }

    // ADMIN/AUTHOR: delete (admin=tout, author=ses articles)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Author")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await FindAccessibleArticle(id);
        if (a is null) return NotFound();

        _db.Articles.Remove(a);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Deleted", id });
    }
}
