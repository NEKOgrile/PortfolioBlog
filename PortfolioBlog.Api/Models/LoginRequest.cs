namespace PortfolioBlog.Api.Models;

public record LoginRequest(
    string Email,
    string Password
);
