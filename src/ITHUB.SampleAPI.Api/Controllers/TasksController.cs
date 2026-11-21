using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ITHUB.SampleAPI.Application.Contracts.Tasks;
using ITHUB.SampleAPI.Application.Services.Tasks;
using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHUB.SampleAPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(ITaskBoardService taskBoardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTasks(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var isAdmin = User.IsInRole(UserRoles.Admin);
        return Ok(await taskBoardService.GetTasksAsync(isAdmin, userId.Value, cancellationToken));
    }

    [HttpGet("{taskId:long}")]
    public async Task<IActionResult> GetTask(long taskId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var task = await taskBoardService.GetTaskAsync(taskId, User.IsInRole(UserRoles.Admin), userId.Value, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType<TaskDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        if (adminId is null)
            return Unauthorized();

        var task = await taskBoardService.CreateTaskAsync(request, adminId.Value, cancellationToken);
        if (task is null)
            return BadRequest(new { message = "The assigned user does not exist or is inactive." });

        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task);
    }

    [HttpPost("{taskId:long}/activities")]
    [ProducesResponseType<TaskActivityDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddActivity(
        long taskId,
        CreateActivityRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var activity = await taskBoardService.AddActivityAsync(
            taskId, request, User.IsInRole(UserRoles.Admin), userId.Value, cancellationToken);
        return activity is null
            ? NotFound()
            : StatusCode(StatusCodes.Status201Created, activity);
    }

    [HttpPatch("{taskId:long}/activities/{activityId:long}/status")]
    public async Task<IActionResult> UpdateActivityStatus(
        long taskId,
        long activityId,
        UpdateActivityStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var updated = await taskBoardService.UpdateActivityStatusAsync(
            taskId, activityId, request.StatusId, User.IsInRole(UserRoles.Admin), userId.Value, cancellationToken);
        return updated
            ? NoContent()
            : NotFound(new { message = "The task or activity was not found, or you cannot change its status." });
    }

    private long? GetUserId()
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return long.TryParse(subject, out var userId) ? userId : null;
    }
}
