using TenantManagement.Application.DTOs.Tenants;
using TenantManagement.Application.Interfaces;
using TenantManagement.Application.Mapping;
using TenantManagement.Core.Entities;
using TenantManagement.Core.Exceptions;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Application.Services;

public sealed class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantResponse> CreateAsync( CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim();

        if (await _unitOfWork.Tenants.SlugExistsAsync(slug, cancellationToken))
        {
            throw new ConflictException("A tenant with the same slug already exists.");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tenant.ToResponse();
    }

    public async Task<TenantResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tenant");

        return tenant.ToResponse();
    }
}
