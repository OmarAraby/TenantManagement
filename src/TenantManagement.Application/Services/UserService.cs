using TenantManagement.Application.DTOs.Users;
using TenantManagement.Application.Interfaces;
using TenantManagement.Application.Mapping;
using TenantManagement.Core.Entities;
using TenantManagement.Core.Exceptions;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Application.Services;

public sealed class UserService : IUserService
{
    private const string UserResource = "User";

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UserService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenantScope();

        var users = await _unitOfWork.Users.ListAsync(cancellationToken);

        return users.ToResponses();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureTenantScope();

        if (await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken) is null)
        {
            throw new NotFoundException("Tenant");
        }

        var email = request.Email.Trim();

        if (await _unitOfWork.Users.EmailExistsAsync(email, null, cancellationToken))
        {
            throw new ConflictException("A user with the same email already exists in this tenant.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            Role = request.Role
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }

    public async Task<UserResponse> UpdateAsync( Guid id, UpdateUserRequest request,CancellationToken cancellationToken = default)
    {
        EnsureTenantScope();

        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(UserResource);

        var email = request.Email.Trim();

        if (await _unitOfWork.Users.EmailExistsAsync(email, id, cancellationToken))
        {
            throw new ConflictException("A user with the same email already exists in this tenant.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Role = request.Role;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTenantScope();

        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(UserResource);

        user.IsActive = false;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }



    // helper method to ensure that the tenant scope is present in the request context   
    private Guid EnsureTenantScope()
    {
        return _tenantContext.TenantId
            ?? throw new TenantScopeException("A tenant is required. Supply a valid X-Tenant-Id header.");
    }
}
