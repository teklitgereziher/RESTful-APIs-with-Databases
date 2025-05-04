using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureCosmos.CRUD.WebAPI.Controllers
{
  /// <summary>
  /// This controller uses client credential flow to authorize.
  /// This can only be accessed by an authorized client that has the “Weather.Read” role.
  /// This role will be read from the role claim in the Access Token.
  /// </summary>
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(Roles = "User.Read")]
  public class UsersController : ControllerBase
  {
    public UsersController() { }

    [HttpGet]
    [Route("user")]
    public IActionResult GetUser()
    {
      // This is a test endpoint to check if the user is authenticated.
      // The user should be authenticated using client credential flow.
      if (User.Identity.IsAuthenticated)
      {
        return Ok("User is authenticated");
      }
      else
      {
        return Unauthorized("User is not authenticated");
      }
    }
  }
}
