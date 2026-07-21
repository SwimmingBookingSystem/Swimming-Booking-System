using SBS.Application.Common.Dtos;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Features.Staff.Commands.CheckOut;
using SBS.Application.Features.Staff.Commands.ManualCheckIn;
using SBS.Application.Features.Staff.Commands.QrCheckIn;
using SBS.Application.Features.Staff.Queries.GetAllBookings;
using SBS.Application.Features.Staff.Queries.GetBookingDetail;
using SBS.Application.Features.Staff.Queries.SearchBookings;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IStaffService
{
    Task<List<BookingListItemDto>> SearchBookingsAsync(StaffSearchBookingsQuery query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<BookingListItemDto>> GetAllBookingsAsync(StaffGetAllBookingsQuery query, CancellationToken cancellationToken = default);
    Task<BookingDetailDto?> GetBookingDetailAsync(StaffGetBookingDetailQuery query, CancellationToken cancellationToken = default);
    Task<StaffCheckInResultDto> QrCheckInAsync(StaffQrCheckInCommand command, CancellationToken cancellationToken = default);
    Task<StaffCheckInResultDto> ManualCheckInAsync(StaffManualCheckInCommand command, CancellationToken cancellationToken = default);
    Task<StaffCheckOutResultDto> CheckOutAsync(StaffCheckOutCommand command, CancellationToken cancellationToken = default);
}
