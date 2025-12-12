namespace PortfolioBlog.Api.Models.Articles;

// Champs optionnels (tu peux envoyer seulement ce que tu veux changer)
public class ArticleUpdateDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsPublished { get; set; }
}
