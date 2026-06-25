using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Manager.TicketTypes.Commands.CloseTicketType;
using SBS.Application.Features.Manager.TicketTypes.Commands.CreateTicketType;
using SBS.Application.Features.Manager.TicketTypes.Commands.OpenTicketType;
using SBS.Application.Features.Manager.TicketTypes.Commands.SeedDefaultTickets;
using SBS.Application.Features.Manager.TicketTypes.Commands.UpdateTicketType;
using SBS.Application.Features.Manager.TicketTypes.Queries.GetTicketTypeById;
using SBS.Application.Features.Manager.TicketTypes.Queries.GetTicketTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Manager;

[ApiController]
[Route("api/manager/ticket-types")]
[Authorize(Roles = "Manager")]
public class ManagerTicketTypeController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<CreateTicketTypeCommand> _createValidator;
    private readonly IValidator<UpdateTicketTypeCommand> _updateValidator;

    public ManagerTicketTypeController(
        ISender mediator,
        IValidator<CreateTicketTypeCommand> createValidator,
        IValidator<UpdateTicketTypeCommand> updateValidator)
    {
        _mediator        = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // GET /api/manager/ticket-types?page=1&pageSize=10&category=Single&status=Active
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page         = 1,
        [FromQuery] int pageSize     = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? status   = null)
        => Ok(await _mediator.Send(new GetTicketTypesQuery(page, pageSize, category, status)));

    // GET /api/manager/ticket-types/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _mediator.Send(new GetTicketTypeByIdQuery(id)));

    // POST /api/manager/ticket-types
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketTypeRequest request)
    {
        var command = new CreateTicketTypeCommand(
            request.TicketCode,
            request.TicketName,
            request.Category,
            request.BasePrice,
            request.DiscountPercent,
            request.Description,
            request.ComboDetails?.Select(d => new ComboDetailRequest
            {
                SingleTicketTypeId = d.SingleTicketTypeId,
                Quantity           = d.Quantity
            }).ToList(),
            request.ApplyToPoolIds);

        var validation = await _createValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.TicketTypeId }, result);
    }

    // PUT /api/manager/ticket-types/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketTypeRequest request)
    {
        var command = new UpdateTicketTypeCommand(
            id,
            request.TicketName,
            request.BasePrice,
            request.DiscountPercent,
            request.Description,
            request.ComboDetails?.Select(d => new ComboDetailRequest
            {
                SingleTicketTypeId = d.SingleTicketTypeId,
                Quantity           = d.Quantity
            }).ToList());

        var validation = await _updateValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        return Ok(await _mediator.Send(command));
    }

    // POST /api/manager/ticket-types/defaults  (Seed 3 vé đơn cố định)
    [HttpPost("defaults")]
    public async Task<IActionResult> SeedDefaults()
        => Ok(await _mediator.Send(new SeedDefaultTicketsCommand()));

    // PATCH /api/manager/ticket-types/{id}/close
    [HttpPatch("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
        => Ok(await _mediator.Send(new CloseTicketTypeCommand(id)));

    // PATCH /api/manager/ticket-types/{id}/open
    [HttpPatch("{id:int}/open")]
    public async Task<IActionResult> Open(int id)
        => Ok(await _mediator.Send(new OpenTicketTypeCommand(id)));
}

// ── Request Models 
public class CreateTicketTypeRequest
{
    public string TicketCode { get; set; } = null!;
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!;       // "Single" | "Combo"
    public decimal BasePrice { get; set; }              // Nhập thẳng cho vé Single
    public decimal DiscountPercent { get; set; }
    public string? Description { get; set; }
    public List<ComboDetailRequestModel>? ComboDetails { get; set; }
    public List<int>? ApplyToPoolIds { get; set; }      // ID bể bơi muốn áp dụng ngay
}

public class UpdateTicketTypeRequest
{
    public string TicketName { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? Description { get; set; }
    public List<ComboDetailRequestModel>? ComboDetails { get; set; }
}

public class ComboDetailRequestModel
{
    public int SingleTicketTypeId { get; set; }
    public int Quantity { get; set; }
}
