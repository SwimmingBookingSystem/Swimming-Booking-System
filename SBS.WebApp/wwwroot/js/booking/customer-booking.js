/**
 * ============================================================================
 * SBS - CUSTOMER BOOKING MODULE (CUSTOMER-BOOKING.JS)
 * ============================================================================
 * Quản lý luồng đặt lịch bơi, tham gia hàng chờ (Waitlist) và hủy hàng chờ
 * dành cho khách hàng (Customer).
 */

$(document).ready(function () {

    // =========================================================================
    // 1. STATE & GLOBAL VARIABLES (BIẾN TRẠNG THÁI)
    // =========================================================================
    let selectedSlotId = null;
    let selectedSlotTime = null;
    let isSelectedSlotFull = false;
    let isSelectedSlotLate = false;
    let isSelectedSlotInWaitlist = false;
    let selectedWaitlistEntryId = null;
    let ticketsData = [];

    // =========================================================================
    // 2. AUTHENTICATION CHECK (KIỂM TRA QUYỀN HẠN TÀI KHOẢN)
    // =========================================================================
    const role = window.USER_ROLE || "";
    if (!role || role.toLowerCase() !== 'customer') {
        window.location.href = '/Auth/Login';
        return;
    }

    // =========================================================================
    // 3. UTILITY HELPER FUNCTIONS (CÁC HÀM BỔ TRỢ HỆ THỐNG)
    // =========================================================================

    /**
     * Lấy giá trị của Cookie theo tên.
     * @param {string} name - Tên cookie cần lấy
     * @returns {string|null} Giá trị cookie hoặc null
     */
    function getCookie(name) {
        const value = "; " + document.cookie;
        const parts = value.split("; " + name + "=");
        if (parts.length === 2) return parts.pop().split(";").shift();
        return null;
    }

    /**
     * Bóc tách thông điệp lỗi chi tiết từ phản hồi HTTP Error của Backend API.
     * Hỗ trợ đọc cả exception đơn lẻ (message/Message) lẫn danh sách lỗi (errors/Errors).
     * @param {object} xhr - Đối tượng XMLHttpRequest từ AJAX error callback
     * @param {string} fallbackMsg - Thông báo mặc định nếu không parse được lỗi
     * @returns {string} Thông điệp lỗi định dạng chuỗi hoàn chỉnh
     */
    function extractErrorMessage(xhr, fallbackMsg) {
        if (!xhr || !xhr.responseJSON) {
            return (xhr && xhr.responseText && xhr.responseText.length < 200) ? xhr.responseText : fallbackMsg;
        }
        const res = xhr.responseJSON;
        if (res.message) return res.message;
        if (res.Message) return res.Message;
        if (res.errors) {
            if (Array.isArray(res.errors)) return res.errors.join('\n');
            if (typeof res.errors === 'object') return Object.values(res.errors).flat().join('\n');
            return res.errors.toString();
        }
        if (res.Errors) {
            if (Array.isArray(res.Errors)) return res.Errors.join('\n');
            if (typeof res.Errors === 'object') return Object.values(res.Errors).flat().join('\n');
            return res.Errors.toString();
        }
        if (res.title) return res.title;
        return fallbackMsg;
    }

    // =========================================================================
    // 4. UI HELPER FUNCTIONS (CÁC HÀM CẬP NHẬT GIAO DIỆN & TÍNH TOÁN)
    // =========================================================================

    /**
     * Cập nhật hiển thị ngày đặt lịch lên khung tổng quan (định dạng dd/mm/yyyy).
     * @param {string} dateStr - Chuỗi ngày định dạng yyyy-mm-dd
     */
    function updateSummaryDate(dateStr) {
        if (!dateStr) return;
        const parts = dateStr.split('-');
        if (parts.length === 3) {
            $('#summary-date').text(`${parts[2]}/${parts[1]}/${parts[0]}`);
        }
    }

    /**
     * Cập nhật thời gian khung giờ (Slot) được chọn lên phần tổng quan.
     */
    function updateSummaryTime() {
        if (selectedSlotTime) {
            $('#summary-time').text(selectedSlotTime).removeClass('text-muted').addClass('text-dark');
        } else {
            $('#summary-time').text('Chưa chọn').removeClass('text-dark').addClass('text-muted');
        }
    }

    /**
     * Thay đổi màu sắc, kiểu dáng và văn bản trên nút bấm Đặt Lịch (Submit) 
     * tùy theo trạng thái ca bơi (Ca thường / Hết chỗ / Đã vào hàng chờ).
     */
    function updateSubmitButtonUI() {
        const btn = $('#btn-submit-booking');
        if (isSelectedSlotInWaitlist) {
            btn.removeClass('btn-primary btn-warning').addClass('btn-danger');
            btn.html('Hủy Hàng Chờ <i class="bi bi-x-circle ms-1"></i>');
        } else if (isSelectedSlotFull) {
            btn.removeClass('btn-primary btn-danger').addClass('btn-warning');
            btn.html('Tham gia danh sách chờ <i class="bi bi-clock-history ms-1"></i>');
        } else {
            btn.removeClass('btn-warning btn-danger').addClass('btn-primary');
            btn.html('Đặt lịch ngay <i class="bi bi-arrow-right-circle ms-1"></i>');
        }
    }

    /**
     * Kiểm tra điều kiện hợp lệ để bật/tắt (enable/disable) nút submit.
     */
    function checkSubmitEnable() {
        let count = 0;
        ticketsData.forEach(ticket => {
            count += parseInt($(`#qty-${ticket.poolTicketTypeId}`).val() || 0);
        });

        if (selectedSlotId && (count > 0 || isSelectedSlotInWaitlist)) {
            $('#btn-submit-booking').prop('disabled', false);
        } else {
            $('#btn-submit-booking').prop('disabled', true);
        }
    }

    /**
     * Tính tổng số tiền và tổng số lượng vé đã chọn, đồng thời cập nhật UI tổng quan.
     */
    function calculateTotal() {
        let total = 0;
        let count = 0;
        ticketsData.forEach(ticket => {
            const qty = parseInt($(`#qty-${ticket.poolTicketTypeId}`).val() || 0);
            if (qty > 0) {
                total += (ticket.price * qty);
                count += qty;
            }
        });

        $('#total-price').text(total.toLocaleString('vi-VN') + 'đ');
        $('#summary-tickets').text(`${count} vé`);
        checkSubmitEnable();
    }

    // =========================================================================
    // 5. API DATA FETCHING & RENDERING (FETCH VÀ HIỂN THỊ DỮ LIỆU TỪ API)
    // =========================================================================

    /**
     * Tải danh sách ca bơi (Slots) trống theo bể bơi và ngày được chọn.
     * @param {string} date - Ngày cần kiểm tra (yyyy-mm-dd)
     */
    function loadAvailableSlots(date) {
        if (!date) return;
        
        $('#slots-empty-msg').addClass('d-none');
        $('#slots-error').addClass('d-none');
        $('#slots-container').empty();
        $('#slots-loader').removeClass('d-none');
        
        // Reset trạng thái ca bơi được chọn
        selectedSlotId = null;
        selectedSlotTime = null;
        isSelectedSlotFull = false;
        isSelectedSlotLate = false;
        isSelectedSlotInWaitlist = false;
        selectedWaitlistEntryId = null;
        
        updateSummaryTime();
        checkSubmitEnable();
        updateSubmitButtonUI();

        $.ajax({
            url: `${window.API_BASE_URL}/api/customer-bookings/pools/${POOL_ID}/available-slots?date=${date}`,
            type: 'GET',
            xhrFields: {
                withCredentials: true
            },
            success: function (slots) {
                $('#slots-loader').addClass('d-none');
                
                if (!slots || slots.length === 0) {
                    $('#slots-empty-msg').removeClass('d-none').text('Không có khung giờ nào trống trong ngày này.');
                    return;
                }

                let html = '';
                const now = new Date();

                slots.forEach(slot => {
                    const slotDateTimeStr = `${date}T${slot.startTime}`;
                    const slotStartDateTime = new Date(slotDateTimeStr);
                    const lateLimitDateTime = new Date(slotStartDateTime.getTime() + 30 * 60000);
                    
                    const isLate = slotStartDateTime <= now && now <= lateLimitDateTime;
                    const isPassed = now > lateLimitDateTime;
                    const isFull = slot.availableCapacity <= 0;
                    const isDisabled = isPassed ? 'disabled' : '';
                    
                    let statusText = `(Còn ${slot.availableCapacity} / ${slot.capacity})`;
                    if (isPassed) {
                        statusText = '(Đã qua)';
                    } else if (slot.isInWaitlist) {
                        statusText = `<span class="text-primary fw-bold">Đã vào hàng chờ (Vị trí: ${slot.waitlistPosition} / ${slot.totalWaitlistCount})</span>`;
                    } else if (isFull) {
                        statusText = `<span class="text-warning fw-bold">(Hết chỗ - ${slot.totalWaitlistCount} người đang chờ)</span>`;
                    } else if (isLate) {
                        statusText = `<span class="text-warning fw-bold">(Còn ${slot.availableCapacity} - Vào trễ)</span>`;
                    }
                    
                    let extraClass = '';
                    if (slot.isInWaitlist) extraClass = 'border-primary';
                    else if (isFull && !isPassed) extraClass = 'border-warning';
                    else if (isLate && !isPassed) extraClass = 'border-info';

                    html += `
                        <div class="col-6 col-sm-4">
                            <button type="button" class="slot-btn ${extraClass}" 
                                data-id="${slot.poolSlotId}" 
                                data-time="${slot.startTime.substring(0,5)} - ${slot.endTime.substring(0,5)}" 
                                data-full="${isFull}" 
                                data-late="${isLate}" 
                                data-inwaitlist="${slot.isInWaitlist}" 
                                data-waitlistid="${slot.waitlistEntryId || ''}" ${isDisabled}>
                                <div class="fs-5 fw-bold">${slot.startTime.substring(0,5)}</div>
                                <div class="small">${slot.endTime.substring(0,5)}</div>
                                <div class="small text-muted mt-1" style="font-size: 0.75rem;">${statusText}</div>
                            </button>
                        </div>
                    `;
                });
                
                $('#slots-container').html(html);

                // Gắn sự kiện click chọn ca bơi
                $('.slot-btn:not(:disabled)').on('click', function () {
                    $('.slot-btn').removeClass('selected');
                    $(this).addClass('selected');
                    
                    selectedSlotId = $(this).data('id');
                    selectedSlotTime = $(this).data('time');
                    isSelectedSlotFull = $(this).data('full');
                    isSelectedSlotLate = $(this).data('late');
                    isSelectedSlotInWaitlist = $(this).data('inwaitlist') === true;
                    selectedWaitlistEntryId = $(this).data('waitlistid');
                    
                    if (isSelectedSlotLate) {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Ca bơi đã bắt đầu!',
                            text: 'Cảnh báo: Ca bơi này đã bắt đầu. Nếu bạn đặt bây giờ, bạn sẽ có ít thời gian bơi hơn.',
                            confirmButtonColor: '#0ea5e9'
                        });
                    }

                    updateSummaryTime();
                    checkSubmitEnable();
                    updateSubmitButtonUI();
                });
            },
            error: function () {
                $('#slots-loader').addClass('d-none');
                $('#slots-error').removeClass('d-none').text('Đã xảy ra lỗi khi tải danh sách giờ. Vui lòng kiểm tra lại kết nối hoặc đăng nhập.');
            }
        });
    }

    /**
     * Tải danh sách các loại vé (Vé Đơn / Combo) áp dụng cho bể bơi hiện tại.
     */
    function loadTickets() {
        $.ajax({
            url: `${window.API_BASE_URL}/api/customer-bookings/pools/${POOL_ID}/tickets`,
            type: 'GET',
            xhrFields: {
                withCredentials: true
            },
            success: function (tickets) {
                $('#tickets-loader').addClass('d-none');
                ticketsData = tickets;

                if (!tickets || tickets.length === 0) {
                    $('#tickets-container').html('<div class="alert alert-warning border-0">Chưa có bảng giá vé cho bể bơi này.</div>');
                    return;
                }

                let singleHtml = '';
                let comboHtml = '';

                tickets.forEach(ticket => {
                    const slotEqText = ticket.slotEquivalent > 1 ? `<div class="small text-warning fw-bold"><i class="bi bi-people-fill"></i> Tương đương ${ticket.slotEquivalent} suất bơi</div>` : '';
                    
                    let actionsHtml = '';
                    let opacityClass = '';
                    let inactiveBadge = '';

                    if (ticket.isActive === false) {
                        opacityClass = 'opacity-50';
                        inactiveBadge = '<div class="small text-danger fw-bold mt-1"><i class="bi bi-exclamation-circle"></i> Vé ngừng hoạt động</div>';
                        actionsHtml = `<span class="badge bg-secondary">Ngừng bán</span>`;
                    } else {
                        actionsHtml = `
                            <div class="d-flex align-items-center gap-2">
                                <button type="button" class="qty-btn qty-minus" data-id="${ticket.poolTicketTypeId}">-</button>
                                <input type="number" class="qty-input" id="qty-${ticket.poolTicketTypeId}" value="0" min="0" max="20" readonly>
                                <button type="button" class="qty-btn qty-plus" data-id="${ticket.poolTicketTypeId}" data-sloteq="${ticket.slotEquivalent}">+</button>
                            </div>
                        `;
                    }

                    const cardHtml = `
                        <div class="ticket-card d-flex justify-content-between align-items-center ${opacityClass}">
                            <div>
                                <h6 class="fw-bold mb-1">${ticket.ticketName}</h6>
                                <div class="text-primary fw-semibold">${Number(ticket.price).toLocaleString('vi-VN')}đ</div>
                                ${slotEqText}
                                ${inactiveBadge}
                            </div>
                            ${actionsHtml}
                        </div>
                    `;

                    if (ticket.category === 'Combo') {
                        comboHtml += cardHtml;
                    } else {
                        singleHtml += cardHtml;
                    }
                });

                if (singleHtml) {
                    $('#single-tickets-section').removeClass('d-none');
                    $('#single-tickets-container').html(singleHtml);
                }
                if (comboHtml) {
                    $('#combo-tickets-section').removeClass('d-none');
                    $('#combo-tickets-container').html(comboHtml);
                }

                // Gắn sự kiện nút Giảm số lượng
                $('.qty-minus').on('click', function () {
                    const id = $(this).data('id');
                    const input = $(`#qty-${id}`);
                    let val = parseInt(input.val());
                    if (val > 0) {
                        input.val(val - 1);
                        calculateTotal();
                    }
                });

                // Gắn sự kiện nút Tăng số lượng (Kiểm tra giới hạn 20 suất bơi)
                $('.qty-plus').on('click', function () {
                    const id = $(this).data('id');
                    const slotEq = parseInt($(this).data('sloteq') || 1);
                    const input = $(`#qty-${id}`);
                    let val = parseInt(input.val());
                    
                    let currentTotalSlots = 0;
                    ticketsData.forEach(t => {
                        currentTotalSlots += parseInt($(`#qty-${t.poolTicketTypeId}`).val() || 0) * (t.slotEquivalent || 1);
                    });

                    if (currentTotalSlots + slotEq > 20) {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Đạt giới hạn',
                            text: 'Bạn chỉ được phép đặt tối đa 20 suất bơi trong một lần giao dịch!',
                            confirmButtonColor: '#0ea5e9'
                        });
                        return;
                    }

                    if (val < 20) {
                        input.val(val + 1);
                        calculateTotal();
                    }
                });
            },
            error: function () {
                $('#tickets-loader').addClass('d-none');
                $('#tickets-container').html('<div class="alert alert-danger border-0">Không thể tải thông tin giá vé.</div>');
            }
        });
    }

    // =========================================================================
    // 6. ACTION EXECUTION HANDLERS (GỬI REQUEST ĐẾN API)
    // =========================================================================

    /**
     * Thực thi gọi API Tạo Đặt Lịch (Create Booking) và chuyển hướng thanh toán PayOS.
     * @param {object} payload - Dữ liệu đặt lịch bơi
     * @param {object} btn - Đối tượng jQuery nút bấm submit
     */
    function executeCreateBooking(payload, btn) {
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

        $.ajax({
            url: `${window.API_BASE_URL}/api/customer-bookings/create`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            xhrFields: {
                withCredentials: true
            },
            success: function (response) {
                if (response.paymentLink) {
                    window.location.href = response.paymentLink;
                } else {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Cảnh báo',
                        text: "Đặt lịch thành công nhưng không tìm thấy link thanh toán.",
                        confirmButtonColor: '#0ea5e9'
                    });
                    btn.html('Đặt lịch ngay <i class="bi bi-arrow-right-circle ms-1"></i>');
                }
            },
            error: function (xhr) {
                btn.prop('disabled', false).html('Đặt lịch ngay <i class="bi bi-arrow-right-circle ms-1"></i>');
                const msg = extractErrorMessage(xhr, "Đã xảy ra lỗi khi tạo đặt lịch. Vui lòng thử lại.");
                Swal.fire({
                    icon: 'error',
                    title: 'Thông báo',
                    text: msg,
                    confirmButtonColor: '#0ea5e9'
                });
            }
        });
    }

    /**
     * Thực thi gọi API Tham Gia Hàng Chờ (Join Waitlist).
     * @param {object} payload - Dữ liệu ca bơi và tổng suất bơi cần chờ
     * @param {object} btn - Đối tượng jQuery nút bấm submit
     */
    function executeJoinWaitlist(payload, btn) {
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

        $.ajax({
            url: `${window.API_BASE_URL}/api/customer-bookings/waitlist/join`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            xhrFields: {
                withCredentials: true
            },
            success: function (response) {
                if (response.succeeded === false) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Thông báo',
                        text: response.message || "Đã xảy ra lỗi khi tham gia danh sách chờ.",
                        confirmButtonColor: '#0ea5e9'
                    });
                    btn.prop('disabled', false).html('Tham gia danh sách chờ <i class="bi bi-clock-history ms-1"></i>');
                    return;
                }
                
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công!',
                    text: response.message || "Tham gia danh sách chờ thành công! Vui lòng kiểm tra email của bạn khi có chỗ trống.",
                    confirmButtonColor: '#0ea5e9'
                }).then(() => {
                    window.location.reload();
                });
            },
            error: function (xhr) {
                btn.prop('disabled', false).html('Tham gia danh sách chờ <i class="bi bi-clock-history ms-1"></i>');
                const msg = extractErrorMessage(xhr, "Đã xảy ra lỗi khi tham gia danh sách chờ.");
                Swal.fire({
                    icon: 'error',
                    title: 'Thông báo',
                    text: msg,
                    confirmButtonColor: '#0ea5e9'
                });
            }
        });
    }

    /**
     * Thực thi gọi API Hủy Tham Gia Hàng Chờ (Cancel Waitlist).
     * @param {number} waitlistEntryId - Mã ID đăng ký hàng chờ
     * @param {object} btn - Đối tượng jQuery nút bấm submit
     */
    function executeCancelWaitlist(waitlistEntryId, btn) {
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

        $.ajax({
            url: `${window.API_BASE_URL}/api/customer-bookings/waitlist/cancel`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ waitlistEntryId: waitlistEntryId }),
            xhrFields: {
                withCredentials: true
            },
            success: function (response) {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: 'Đã hủy tham gia danh sách chờ thành công.',
                    confirmButtonColor: '#0ea5e9'
                }).then(() => {
                    window.location.reload();
                });
            },
            error: function (xhr) {
                btn.prop('disabled', false).html('Hủy Hàng Chờ <i class="bi bi-x-circle ms-1"></i>');
                const msg = extractErrorMessage(xhr, "Đã xảy ra lỗi khi hủy danh sách chờ.");
                Swal.fire({
                    icon: 'error',
                    title: 'Thông báo',
                    text: msg,
                    confirmButtonColor: '#0ea5e9'
                });
            }
        });
    }

    // =========================================================================
    // 7. MAIN SUBMISSION DISPATCHER & EVENTS (SỰ KIỆN NÚT BẤM VÀ ĐIỀU HƯỚNG)
    // =========================================================================

    // Đăng ký sự kiện click nút bấm Submit chính
    $('#btn-submit-booking').on('click', function () {
        const btn = $(this);
        
        let count = 0;
        let totalSlots = 0;
        const tickets = [];
        ticketsData.forEach(ticket => {
            const qty = parseInt($(`#qty-${ticket.poolTicketTypeId}`).val() || 0);
            if (qty > 0) {
                count += qty;
                totalSlots += qty * (ticket.slotEquivalent || 1);
                tickets.push({
                    poolTicketTypeId: ticket.poolTicketTypeId,
                    quantity: qty
                });
            }
        });

        // 1. Trường hợp: Hủy đăng ký hàng chờ
        if (isSelectedSlotInWaitlist) {
            Swal.fire({
                title: 'Hủy đăng ký hàng chờ?',
                text: "Bạn có chắc chắn muốn hủy bỏ tham gia hàng chờ cho ca bơi này?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Đồng ý hủy',
                cancelButtonText: 'Không'
            }).then((result) => {
                if (result.isConfirmed) {
                    executeCancelWaitlist(selectedWaitlistEntryId, btn);
                }
            });
        } 
        // 2. Trường hợp: Ca bơi đầy -> Đăng ký hàng chờ
        else if (isSelectedSlotFull) {
            Swal.fire({
                title: 'Ca bơi đã đầy',
                text: "Bạn có muốn tham gia danh sách chờ? Nếu có người hủy vé, chúng tôi sẽ thông báo cho bạn qua Email.",
                icon: 'info',
                showCancelButton: true,
                confirmButtonColor: '#eab308',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Tham gia hàng chờ',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    const payload = {
                        poolSlotId: selectedSlotId,
                        quantity: totalSlots
                    };
                    executeJoinWaitlist(payload, btn);
                }
            });
        } 
        // 3. Trường hợp: Đặt lịch thông thường
        else {
            const payload = {
                poolSlotId: selectedSlotId,
                tickets: tickets
            };
            executeCreateBooking(payload, btn);
        }
    });

    // Lắng nghe sự kiện thay đổi ngày chọn đặt lịch
    $('#booking-date').on('change', function () {
        loadAvailableSlots($(this).val());
        updateSummaryDate($(this).val());
    });

    // =========================================================================
    // 8. INITIALIZATION ON LOAD (KHỞI TẠO DỮ LIỆU BAN ĐẦU)
    // =========================================================================
    loadTickets();
    loadAvailableSlots($('#booking-date').val());
    updateSummaryDate($('#booking-date').val());

});
