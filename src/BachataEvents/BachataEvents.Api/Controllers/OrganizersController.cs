using System.Security.Claims;
using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Festivals;
using BachataEvents.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachataEvents.Api.Controllers;

[ApiController]
[Route("api/organizers")]
public sealed class OrganizersController : ControllerBase
{
    private readonly IFestivalService _festivals;

    public OrganizersController(IFestivalService festivals)
    {
        _festivals = festivals;
    }

    [Authorize(Roles = AppRoles.Organizer)]
    [HttpGet("me/festivals")]
    public async Task<ActionResult<IReadOnlyList<FestivalListItemDto>>> GetMine(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _festivals.GetMineAsync(userId, ct);
        return Ok(result);
    }
}
