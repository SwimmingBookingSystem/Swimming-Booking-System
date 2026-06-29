using FluentValidation;
using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Commands.UpdatePool;

// Command 
public record UpdatePoolCommand(
    int PoolId,
    string PoolName,
    string Address,
    string? Description,
    List<PoolImageItem>? Images,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime,
    double Area
) : IRequest<PoolDto>;

// Handler
public class UpdatePoolCommandHandler : IRequestHandler<UpdatePoolCommand, PoolDto>
{
    private readonly IUnitOfWork _uow;

    public UpdatePoolCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PoolDto> Handle(UpdatePoolCommand request, CancellationToken ct)
    {
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query()
                .Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        pool.PoolName    = request.PoolName;
        pool.Address     = request.Address;
        pool.Description = request.Description;
        pool.OpeningTime = request.OpeningTime;
        pool.ClosingTime = request.ClosingTime;
        pool.Area        = request.Area;
        pool.StandardCapacity = (int)(request.Area / 2.5);
        pool.UpdatedAt   = DateTime.UtcNow;

        // Xử lý cập nhật danh sách ảnh nếu có truyền lên
        if (request.Images != null)
        {
            // Xóa ảnh cũ
            var oldImages = await _uow.ToListAsync(
                _uow.Repository<PoolImage>().Query().Where(img => img.PoolId == request.PoolId), ct);
            
            _uow.Repository<PoolImage>().DeleteRange(oldImages);

            // Thêm ảnh mới
            if (request.Images.Any())
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
        }

        _uow.Repository<Pool>().Update(pool);
        await _uow.SaveChangesAsync(ct);

        return new PoolDto
        {
            PoolId      = pool.PoolId,
            PoolName    = pool.PoolName,
            Address     = pool.Address,
            Description = pool.Description,
            Images      = pool.PoolImages.Select(img => new PoolImageDto 
            {
                PoolImageId = img.PoolImageId,
                ImageUrl    = img.ImageUrl,
                IsCover     = img.IsCover,
                SortOrder   = img.SortOrder,
                CreatedAt   = img.CreatedAt
            }).ToList(),
            OpeningTime = pool.OpeningTime.ToString(@"hh\:mm"),
            ClosingTime = pool.ClosingTime.ToString(@"hh\:mm"),
            Status      = pool.Status,
            Area        = pool.Area,
            StandardCapacity = pool.StandardCapacity,
            CreatedAt   = pool.CreatedAt,
            UpdatedAt   = pool.UpdatedAt
        };
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class UpdatePoolCommandValidator : AbstractValidator<UpdatePoolCommand>
{
    public UpdatePoolCommandValidator()
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
