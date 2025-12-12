namespace PortfolioBlog.Api.Models.Articles;

public record ArticleCreateDto(string Title, string Content, bool IsPublished);
