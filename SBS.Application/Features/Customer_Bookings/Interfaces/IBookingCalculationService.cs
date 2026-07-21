using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Interfaces;

public interface IBookingCalculationService
{
    /// <summary>
    /// Tính số lượng suất bơi quy đổi cho 1 loại vé (Vé đơn = 1, Vé Combo = Tổng số lượng các item bên trong)
    /// </summary>
    int CalculateSlotEquivalent(TicketType ticketType);

    /// <summary>
    /// Tính tổng số chỗ (capacity) đã được đặt (Booked) cho một PoolSlot
    /// </summary>
    Task<int> GetBookedCapacityAsync(int poolSlotId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tính số chỗ còn trống (Available Capacity) của một PoolSlot
    /// </summary>
    Task<int> GetAvailableCapacityAsync(int poolSlotId, int totalCapacity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tính toán tổng số suất bơi yêu cầu từ danh sách vé khách chọn
    /// </summary>
    int CalculateTotalRequestedSlots(IEnumerable<BookingTicketDto> requestedTickets, IEnumerable<PoolTicketType> ticketTypes);

    /// <summary>
    /// Tính giá tiền chi tiết và tạo danh sách BookingDetail
    /// </summary>
    (decimal TotalAmount, List<BookingDetail> Details) CalculateBookingAmount(
        IEnumerable<BookingTicketDto> requestedTickets, 
        IEnumerable<PoolTicketType> ticketTypes);
}
