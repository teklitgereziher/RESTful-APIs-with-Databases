using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Postgres.CRUD.WebAPI.IntegrationTests.SetupPostgres
{
  public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
  {
    public MockAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : base(options, logger, encoder)
    { }

    protected Claim[] CreateClaims() => [new Claim(ClaimTypes.Name, "Test user")];

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
      var claims = CreateClaims();

      var identity = new ClaimsIdentity(claims, "Test");
      var principal = new ClaimsPrincipal(identity);
      var ticket = new AuthenticationTicket(principal, "Test");

      var result = AuthenticateResult.Success(ticket);
      return Task.FromResult(result);
    }
  }
}
