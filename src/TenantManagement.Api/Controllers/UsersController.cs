using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TenantManagement.Application.Common;
using TenantManagement.Application.DTOs.Users;
using TenantManagement.Application.Interfaces;

namespace TenantManagement.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<Ok<ApiResponse<IReadOnlyList<UserResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);

        return TypedResults.Ok(ApiResponse<IReadOnlyList<UserResponse>>.SuccessResponse(users, "Users retrieved successfully"));
    }

    [HttpPost]
    public async Task<Created<ApiResponse<UserResponse>>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);

        return TypedResults.Created($"/api/users/{user.Id}",
            ApiResponse<UserResponse>.SuccessResponse(user, "User created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<Ok<ApiResponse<UserResponse>>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateAsync(id, request, cancellationToken);

        return TypedResults.Ok(ApiResponse<UserResponse>.SuccessResponse(user, "User updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<Ok<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);

        return TypedResults.Ok(ApiResponse.SuccessResponse("User deleted successfully"));
    }
}
