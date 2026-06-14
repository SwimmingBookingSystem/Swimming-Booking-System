using FluentValidation;
using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System;
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
    string? ImageUrl, // Giữ lại để tương thích
    List<PoolImageItem>? Images,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime
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
        pool.ImageUrl    = request.ImageUrl;
        pool.OpeningTime = request.OpeningTime;
        pool.ClosingTime = request.ClosingTime;
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
                    await _uow.Repository<PoolImage>().AddAsync(new PoolImage
                    {
                        PoolId    = pool.PoolId,
                        ImageUrl  = img.ImageUrl,
                        IsCover   = img.IsCover,
                        SortOrder = img.SortOrder == 0 ? index : img.SortOrder,
                        CreatedAt = DateTime.UtcNow
                    }, ct);
                    index++;
                }
            }
        }
        else if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            // Fallback: nếu họ chỉ dùng ImageUrl (chỉ thêm nếu chưa có ảnh nào)
            var existingImages = await _uow.AnyAsync(
                _uow.Repository<PoolImage>().Query().Where(img => img.PoolId == request.PoolId), ct);
                
            if (!existingImages)
            {
                await _uow.Repository<PoolImage>().AddAsync(new PoolImage
                {
                    PoolId    = pool.PoolId,
                    ImageUrl  = request.ImageUrl,
                    IsCover   = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow
                }, ct);
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
            ImageUrl    = pool.ImageUrl,
            OpeningTime = pool.OpeningTime.ToString(@"hh\:mm"),
            ClosingTime = pool.ClosingTime.ToString(@"hh\:mm"),
            Status      = pool.Status,
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
            .WithMessage("Mỗi bể bơi chỉ được phép có tối đa 10 ảnh.");

        RuleForEach(x => x.Images)
            .ChildRules(img =>
            {
                img.RuleFor(i => i.ImageUrl)
                   .NotEmpty().WithMessage("URL ảnh không được để trống.")
                   .MaximumLength(1000).WithMessage("URL ảnh không được vượt quá 1000 ký tự.");
            });
    }
}
