using ITHUB.SampleAPI.Application.Services.Tasks;
using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHUB.SampleAPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/statuses")]
public sealed class StatusesController(ITaskBoardService taskBoardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStatuses([FromQuery] string scope, CancellationToken cancellationToken)
    {
        if (string.Equals(scope, StatusCatalogConstants.TaskScope, StringComparison.OrdinalIgnoreCase))
            scope = StatusCatalogConstants.TaskScope;
        else if (string.Equals(scope, StatusCatalogConstants.ActivityScope, StringComparison.OrdinalIgnoreCase))
            scope = StatusCatalogConstants.ActivityScope;
        else
            return BadRequest(new { message = "Scope must be Task or Activity." });

        return Ok(await taskBoardService.GetStatusesAsync(scope, cancellationToken));
    }
}
