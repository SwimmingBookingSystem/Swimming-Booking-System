using FluentValidation;
using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Pools.Commands.ManagePoolImages;

// ── Command ───────────────────────────────────────────────────────────────────
/// <summary>
/// Cập nhật toàn bộ danh sách ảnh của pool (Replace strategy):
/// - Xóa tất cả ảnh cũ
/// - Thêm lại danh sách ảnh mới
/// - Đảm bảo chỉ có 1 ảnh IsCover = true
/// </summary>
public record UpdatePoolImagesCommand(
    int PoolId,
    List<PoolImageItem> Images
) : IRequest<List<PoolImageDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────
public class UpdatePoolImagesCommandHandler
    : IRequestHandler<UpdatePoolImagesCommand, List<PoolImageDto>>
{
    private readonly IUnitOfWork _uow;

    public UpdatePoolImagesCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<PoolImageDto>> Handle(
        UpdatePoolImagesCommand request, CancellationToken ct)
    {
        // 1. Kiểm tra pool tồn tại
        bool poolExists = await _uow.AnyAsync(
            _uow.Repository<Pool>().Query()
                .Where(p => p.PoolId == request.PoolId), ct);

        if (!poolExists)
            throw new NotFoundException(nameof(Pool), request.PoolId);

        // 2. Validate: tối đa 10 ảnh
        if (request.Images.Count > 10)
            throw new BadRequestException("Mỗi bể bơi chỉ được phép có tối đa 10 ảnh.");

        // 3. Validate: không có URL trùng nhau trong request
        var duplicates = request.Images
            .GroupBy(i => i.ImageUrl)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
            throw new BadRequestException(
                $"Danh sách ảnh có URL bị trùng: {string.Join(", ", duplicates)}");

        // 4. Validate: chỉ 1 ảnh IsCover = true
        int coverCount = request.Images.Count(i => i.IsCover);
        if (coverCount > 1)
            throw new BadRequestException("Chỉ được phép có 1 ảnh bìa (IsCover = true).");

        // 5. Xóa tất cả ảnh cũ của pool
        var oldImages = await _uow.ToListAsync(
            _uow.Repository<PoolImage>().Query()
                .Where(img => img.PoolId == request.PoolId), ct);

        _uow.Repository<PoolImage>().DeleteRange(oldImages);

        // 6. Thêm ảnh mới
        // Nếu không có ảnh nào IsCover, tự đặt ảnh đầu tiên làm cover
        if (request.Images.Any() && coverCount == 0)
            request.Images[0].IsCover = true;

        var newImages = request.Images.Select((img, index) => new PoolImage
        {
            PoolId    = request.PoolId,
            ImageUrl  = img.ImageUrl,
            IsCover   = img.IsCover,
            SortOrder = img.SortOrder == 0 ? index + 1 : img.SortOrder,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        foreach (var img in newImages)
            await _uow.Repository<PoolImage>().AddAsync(img, ct);

        await _uow.SaveChangesAsync(ct);

        return newImages.Select(img => new PoolImageDto
        {
            PoolImageId = img.PoolImageId,
            ImageUrl    = img.ImageUrl,
            IsCover     = img.IsCover,
            SortOrder   = img.SortOrder,
            CreatedAt   = img.CreatedAt
        }).ToList();
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class UpdatePoolImagesCommandValidator : AbstractValidator<UpdatePoolImagesCommand>
{
    public UpdatePoolImagesCommandValidator()
    {
        RuleFor(x => x.Images)
            .NotNull().WithMessage("Danh sách ảnh không được null.");

        RuleForEach(x => x.Images)
            .ChildRules(img =>
            {
                img.RuleFor(i => i.ImageUrl)
                   .NotEmpty().WithMessage("URL ảnh không được để trống.")
                   .MaximumLength(1000).WithMessage("URL ảnh không được vượt quá 1000 ký tự.")
                   .Must(url => url.StartsWith("http://") || url.StartsWith("https://"))
                   .WithMessage("URL ảnh phải bắt đầu bằng http:// hoặc https://");
            });
    }
}
