using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Manager.Pools.Commands.ClosePool;
using SBS.Application.Features.Manager.Pools.Commands.CreatePool;
using SBS.Application.Features.Manager.Pools.Commands.ManagePoolImages;
using SBS.Application.Features.Manager.Pools.Commands.ReopenPool;
using SBS.Application.Features.Manager.Pools.Commands.UpdatePool;
using SBS.Application.Features.Manager.Pools.Dtos;
using SBS.Application.Features.Manager.Pools.Queries.GetPoolById;
using SBS.Application.Features.Manager.Pools.Queries.GetPools;
using SBS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Manager;

[ApiController]
[Route("api/manager/pools")]
[Authorize(Roles = "Manager")]
public class ManagerPoolController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<CreatePoolCommand> _createValidator;
    private readonly IValidator<UpdatePoolCommand> _updateValidator;
    private readonly IValidator<UpdatePoolImagesCommand> _imagesValidator;
    private readonly ICloudinaryService _cloudinaryService;

    public ManagerPoolController(
        ISender mediator,
        IValidator<CreatePoolCommand> createValidator,
        IValidator<UpdatePoolCommand> updateValidator,
        IValidator<UpdatePoolImagesCommand> imagesValidator,
        ICloudinaryService cloudinaryService)
    {
        _mediator        = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _imagesValidator = imagesValidator;
        _cloudinaryService = cloudinaryService;
    }

    /// Lấy danh sách bể bơi (có phân trang, lọc theo status)
    [HttpGet]
    public async Task<IActionResult> GetPools(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
        => Ok(await _mediator.Send(new GetPoolsQuery(page, pageSize, status, search)));

    /// Lấy chi tiết một bể bơi theo ID
    [HttpGet("{poolId:int}")]
    public async Task<IActionResult> GetPool(int poolId)
        => Ok(await _mediator.Send(new GetPoolByIdQuery(poolId)));

    // Upload ảnh lên Cloudinary
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var url = await _cloudinaryService.UploadImageAsync(stream, file.FileName);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Tạo bể bơi mới
    [HttpPost]
    public async Task<IActionResult> CreatePool([FromBody] CreatePoolRequest request)
    {
        // Parse time strings → TimeSpan
        if (!TimeSpan.TryParse(request.OpeningTime, out var openingTime))
            return BadRequest(new { message = "OpeningTime không hợp lệ. Định dạng: HH:mm" });

        if (!TimeSpan.TryParse(request.ClosingTime, out var closingTime))
            return BadRequest(new { message = "ClosingTime không hợp lệ. Định dạng: HH:mm" });

        var command = new CreatePoolCommand(
            request.PoolName, request.Address, request.Description,
            request.Images?.Select(i => new PoolImageItem 
            { 
                ImageUrl = i.ImageUrl, 
                IsCover = i.IsCover, 
                SortOrder = i.SortOrder 
            }).ToList(),
            openingTime, closingTime, request.Area);

        var validation = await _createValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPool), new { poolId = result.PoolId }, result);
    }

    // Cập nhật thông tin bể bơi
    [HttpPut("{poolId:int}")]
    public async Task<IActionResult> UpdatePool(int poolId, [FromBody] UpdatePoolRequest request)
    {
        if (!TimeSpan.TryParse(request.OpeningTime, out var openingTime))
            return BadRequest(new { message = "OpeningTime không hợp lệ. Định dạng: HH:mm" });

        if (!TimeSpan.TryParse(request.ClosingTime, out var closingTime))
            return BadRequest(new { message = "ClosingTime không hợp lệ. Định dạng: HH:mm" });

        var command = new UpdatePoolCommand(
            poolId, request.PoolName, request.Address, request.Description,
            request.Images?.Select(i => new PoolImageItem 
            { 
                ImageUrl = i.ImageUrl, 
                IsCover = i.IsCover, 
                SortOrder = i.SortOrder 
            }).ToList(),
            openingTime, closingTime, request.Area);

        var validation = await _updateValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        return Ok(await _mediator.Send(command));
    }

    // Tạm đóng bể bơi
    [HttpPatch("{poolId:int}/close")]
    public async Task<IActionResult> ClosePool(int poolId)
        => Ok(await _mediator.Send(new ClosePoolCommand(poolId)));

    /// Mở lại bể bơi đã đóng
    [HttpPatch("{poolId:int}/reopen")]
    public async Task<IActionResult> ReopenPool(int poolId)
        => Ok(await _mediator.Send(new ReopenPoolCommand(poolId)));

    // Cập nhật danh sách ảnh bể bơi (thay thế toàn bộ)
    [HttpPut("{poolId:int}/images")]
    public async Task<IActionResult> UpdateImages(
        int poolId, [FromBody] List<PoolImageRequest> images)
    {
        var command = new UpdatePoolImagesCommand(
            poolId,
            images.Select(i => new PoolImageItem
            {
                ImageUrl  = i.ImageUrl,
                IsCover   = i.IsCover,
                SortOrder = i.SortOrder
            }).ToList());

        var validation = await _imagesValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        return Ok(await _mediator.Send(command));
    }
}

// Request Models 
public class CreatePoolRequest
{
    public string PoolName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public List<PoolImageRequest>? Images { get; set; }
    public string OpeningTime { get; set; } = null!;  // "HH:mm"
    public string ClosingTime { get; set; } = null!;  // "HH:mm"
    public double Area { get; set; }
}

public class UpdatePoolRequest
{
    public string PoolName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public List<PoolImageRequest>? Images { get; set; }
    public string OpeningTime { get; set; } = null!;
    public string ClosingTime { get; set; } = null!;
    public double Area { get; set; }
}

// Request model dùng cho PUT /{poolId}/images
public class PoolImageRequest
{
    public string ImageUrl { get; set; } = null!;
    public bool IsCover { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
