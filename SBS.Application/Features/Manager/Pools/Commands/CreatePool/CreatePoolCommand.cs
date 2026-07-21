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

// ── Command
public record CreatePoolCommand(
    string PoolName,
    string Address,
    string? Description,
    List<PoolImageItem>? Images,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime,
    double Area
) : IRequest<CreatePoolResponse>;

// ── Handler 
public class CreatePoolCommandHandler : IRequestHandler<CreatePoolCommand, CreatePoolResponse>
{
    private readonly SBS.Application.Features.Manager.Services.Interfaces.IPoolManagementService _poolService;

    public CreatePoolCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.IPoolManagementService poolService) => _poolService = poolService;

    public async Task<CreatePoolResponse> Handle(CreatePoolCommand request, CancellationToken ct)
    {
        return await _poolService.CreatePoolAsync(request, ct);
    }
}

// ── Validator
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

