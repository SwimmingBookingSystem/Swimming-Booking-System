using FluentValidation;
using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Commands.CreatePool;

// ── Command ──────────────────────────────────────────────────────────────────
public record CreatePoolCommand(
    string PoolName,
    string Address,
    string? Description,
    List<PoolImageItem>? Images,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime,
    double Area
) : IRequest<CreatePoolResponse>;

// ── Handler ───────────────────────────────────────────────────────────────────
public class CreatePoolCommandHandler : IRequestHandler<CreatePoolCommand, CreatePoolResponse>
{
    private readonly IUnitOfWork _uow;

    public CreatePoolCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CreatePoolResponse> Handle(CreatePoolCommand request, CancellationToken ct)
    {
        var pool = new Pool
        {
            PoolName    = request.PoolName,
            Address     = request.Address,
            Description = request.Description,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
            Area        = request.Area,
            StandardCapacity = (int)(request.Area / 2.5),
            Status      = "Active",
            CreatedAt   = DateTime.UtcNow
        };

        // Thêm ảnh nếu có
        if (request.Images != null && request.Images.Any())
        {
            var coverCount = request.Images.Count(i => i.IsCover);
            if (coverCount == 0) request.Images[0].IsCover = true;

            int index = 1;
            foreach (var img in request.Images)
            {
                pool.PoolImages.Add(new PoolImage
                {
                    ImageUrl  = img.ImageUrl,
                    IsCover   = img.IsCover,
                    SortOrder = img.SortOrder == 0 ? index : img.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
                index++;
            }
        }

        await _uow.Repository<Pool>().AddAsync(pool, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreatePoolResponse
        {
            PoolId   = pool.PoolId,
            PoolName = pool.PoolName,
            Status   = pool.Status
        };
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class CreatePoolCommandValidator : AbstractValidator<CreatePoolCommand>
{
    public CreatePoolCommandValidator()
    {
        RuleFor(x => x.PoolName)
            .NotEmpty().WithMessage("Tên bể bơi không được để trống.")
            .MaximumLength(200).WithMessage("Tên bể bơi không được vượt quá 200 ký tự.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.")
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).When(x => x.Description != null);

        RuleFor(x => x.ClosingTime)
            .GreaterThan(x => x.OpeningTime)
            .WithMessage("Giờ đóng cửa phải lớn hơn giờ mở cửa.");

        RuleFor(x => x.Images)
            .Must(imgs => imgs == null || imgs.Count <= 10)
            .WithMessage("Mỗi bể bơi chỉ được phép có tối đa 10 ảnh.")
            .Must(imgs => imgs == null || imgs.Count(i => i.IsCover) <= 1)
            .WithMessage("Chỉ được phép chọn tối đa 1 ảnh làm ảnh bìa.");

        RuleForEach(x => x.Images)
            .ChildRules(img =>
            {
                img.RuleFor(i => i.ImageUrl)
                   .NotEmpty().WithMessage("URL ảnh không được để trống.")
                   .MaximumLength(1000).WithMessage("URL ảnh không được vượt quá 1000 ký tự.");
            });
    }
}
