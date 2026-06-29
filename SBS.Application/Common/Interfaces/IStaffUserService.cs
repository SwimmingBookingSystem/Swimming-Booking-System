using SBS.Application.Common.Dtos.Staff;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

/// <summary>
/// Interface riêng cho Staff — tách biệt với IIdentityService của team auth.
/// Cung cấp khả năng tra cứu thông tin User phục vụ các nghiệp vụ của Staff:
///   - Enrich thông tin khách hàng vào danh sách/chi tiết booking
///   - Tìm kiếm booking theo phone/email của khách
///   - Kiểm tra quyền truy cập hồ bơi theo phân công
/// </summary>
public interface IStaffUserService
{
    /// <summary>
    /// Lấy thông tin ngắn gọn của một user (tên, email, phone).
    /// Dùng để enrich dữ liệu booking với thông tin khách hàng.
    /// Trả về null nếu user không tồn tại.
    /// </summary>
    Task<UserBriefDto?> GetUserBriefAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm danh sách UserId khớp với số điện thoại hoặc email.
    /// Dùng cho Staff khi tìm kiếm booking theo thông tin liên lạc của khách hàng.
    /// Có thể truyền vào cả phone lẫn email, kết quả là OR (tìm theo bất kỳ).
    /// </summary>
    Task<List<Guid>> FindUserIdsByPhoneOrEmailAsync(
        string? phone,
        string? email,
        CancellationToken cancellationToken = default);

    Task<List<int>> GetAssignedPoolIdsAsync(Guid staffId, CancellationToken cancellationToken = default);

    Task<bool> IsStaffAssignedToPoolAsync(Guid staffId, int poolId, CancellationToken cancellationToken = default);
}
