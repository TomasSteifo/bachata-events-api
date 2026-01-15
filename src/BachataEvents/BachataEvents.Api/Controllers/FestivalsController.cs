using System.Security.Claims;
using BachataEvents.Application.Abstractions;
using BachataEvents.Application.Common;
using BachataEvents.Application.Festivals;
using BachataEvents.Application.Validation;
using BachataEvents.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachataEvents.Api.Controllers;

[ApiController]
[Route("api/festivals")]
public sealed class FestivalsController : ControllerBase
{
    private readonly IFestivalService _festivals;
    private readonly IValidator<CreateFestivalRequest> _createValidator;
    private readonly IValidator<UpdateFestivalRequest> _updateValidator;
    private readonly IValidator<FestivalQuery> _queryValidator;
    private readonly IValidator<PublishFestivalRequest> _publishValidator;

    public FestivalsController(
        IFestivalService festivals,
        IValidator<CreateFestivalRequest> createValidator,
        IValidator<UpdateFestivalRequest> updateValidator,
        IValidator<FestivalQuery> queryValidator,
        IValidator<PublishFestivalRequest> publishValidator)
    {
        _festivals = festivals;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
        _publishValidator = publishValidator;
    }

    // PUBLIC
    [HttpGet]
    public async Task<ActionResult<PagedResult<FestivalListItemDto>>> Get(
        [FromQuery] string? country,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] string? q,
        [FromQuery] FestivalSortBy sortBy = FestivalSortBy.StartDateAsc,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new FestivalQuery(country, startDate, endDate, q, sortBy, page, pageSize);
        await _queryValidator.ValidateOrThrowAsync(query, ct);

        var result = await _festivals.GetPublishedAsync(query, ct);
        return Ok(result);
    }

    // PUBLIC: only if published
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FestivalDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var item = await _festivals.GetPublishedByIdAsync(id, ct);
        if (item is null) return NotFound();
        return Ok(item);
    }

    // ORGANIZER
    [Authorize(Roles = AppRoles.Organizer)]
    [HttpPost]
    public async Task<ActionResult<FestivalDetailsDto>> Create([FromBody] CreateFestivalRequest request, CancellationToken ct)
    {
        await _createValidator.ValidateOrThrowAsync(request, ct);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var created = await _festivals.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = AppRoles.Organizer)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FestivalDetailsDto>> Update(Guid id, [FromBody] UpdateFestivalRequest request, CancellationToken ct)
    {
        await _updateValidator.ValidateOrThrowAsync(request, ct);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updated = await _festivals.UpdateAsync(userId, id, request, ct);
        return Ok(updated);
    }

    [Authorize(Roles = AppRoles.Organizer)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _festivals.DeleteAsync(userId, id, ct);
        return NoContent();
    }

    [Authorize(Roles = AppRoles.Organizer)]
    [HttpPatch("{id:guid}/publish")]
    public async Task<ActionResult<FestivalDetailsDto>> Publish(Guid id, [FromBody] PublishFestivalRequest request, CancellationToken ct)
    {
        await _publishValidator.ValidateOrThrowAsync(request, ct);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updated = await _festivals.SetPublishAsync(userId, id, request.IsPublished, ct);
        return Ok(updated);
    }
}
