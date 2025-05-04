namespace AzureCosmos.CRUD.WebAPI.Middlewares
{
  /// <summary>
  /// Access tokens issued to a user have the scp claim.
  /// Access tokens issued to an application have the roles claim.
  /// Access tokens that contain both claims are issued only to users,
  /// where the scp claim designates the delegated permissions,
  /// while the roles claim designates the user's role.
  /// </summary>
  public class UserOrAppTokenMiddleware
  {
    private readonly RequestDelegate next;

    private const string IdtypClaimType = "idtyp";
    private const string IdtypClaimValueApp = "app";
    private const string RolesClaimType = "roles";
    private const string ScpClaimType = "scp";

    public UserOrAppTokenMiddleware(RequestDelegate next)
    {
      this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var user = context.User;
      if (context.User.Identity.IsAuthenticated)
      {
        // Check access token is an app token. (If the idtyp claim enabled)  
        if (context.User.Claims.Any(c => c.Type == IdtypClaimType && c.Value == IdtypClaimValueApp))
        {
          await next(context);
        }
        else if (context.User.Claims.Any(c => c.Type == RolesClaimType)
          && !context.User.Claims.Any(c => c.Type == ScpClaimType)) // Check access token is an app + user token. (If the idtyp claim enabled)  
        {
          await next(context);
        }
        else if (context.User.Claims.Any(c => c.Type == RolesClaimType)
          && context.User.Claims.Any(c => c.Type == ScpClaimType)) // Check access token is a user token. (If the idtyp claim enabled)  
        {
          await next(context);
        }
        else
        {
          context.Response.StatusCode = StatusCodes.Status403Forbidden;
          await context.Response.WriteAsync("Forbidden");
        }
      }
      else
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
      }
    }
  }
}
