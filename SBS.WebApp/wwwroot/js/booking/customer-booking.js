$(document).ready(function () {
    let selectedSlotId = null;
    let selectedSlotTime = null;
    let isSelectedSlotFull = false;
    let isSelectedSlotLate = false;
    let ticketsData = [];

    // Lấy cookie
    function getCookie(name) {
        let value = "; " + document.cookie;
        let parts = value.split("; " + name + "=");
        if (parts.length === 2) return parts.pop().split(";").shift();
        return null;
    }

    const role = getCookie("role");
    if (!role || role.toLowerCase() !== 'customer') {
        // Redirect to login if not customer
        window.location.href = '/Auth/Login';
        return;
    }

    // 1. Khởi tạo: Load Tickets
    loadTickets();

    // Lắng nghe đổi ngày -> Fetch slots
    $('#booking-date').on('change', function () {
        loadAvailableSlots($(this).val());
        updateSummaryDate($(this).val());
    });

    // Mặc định load slots cho ngày hôm nay
    loadAvailableSlots($('#booking-date').val());
    updateSummaryDate($('#booking-date').val());

    // ----------------- FUNCTIONS -----------------

    function updateSummaryDate(dateStr) {
        const parts = dateStr.split('-');
        if (parts.length === 3) {
            $('#summary-date').text(`${parts[2]}/${parts[1]}/${parts[0]}`);
        }
    }

    function loadAvailableSlots(date) {
        if (!date) return;
        
        $('#slots-empty-msg').addClass('d-none');
        $('#slots-error').addClass('d-none');
        $('#slots-container').empty();
        $('#slots-loader').removeClass('d-none');
        
        // Reset selected slot
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
            url: `${API_BASE_URL}/api/customer-bookings/pools/${POOL_ID}/available-slots?date=${date}`,
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
                    
                    // Allow late booking up to 30 mins
                    const lateLimitDateTime = new Date(slotStartDateTime.getTime() + 30 * 60000);
                    
                    const isLate = slotStartDateTime <= now && now <= lateLimitDateTime;
                    const isPassed = now > lateLimitDateTime;
                    const isFull = slot.availableCapacity <= 0;

                    // Chỉ vô hiệu hóa nếu đã qua (kể cả quá 30p)
                    let isDisabled = isPassed ? 'disabled' : '';
                    
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
                    
                    // Thêm class đặc biệt nếu full hoặc late
                    let extraClass = '';
                    if (slot.isInWaitlist) extraClass = 'border-primary';
                    else if (isFull && !isPassed) extraClass = 'border-warning';
                    else if (isLate && !isPassed) extraClass = 'border-info';

                    html += `
                        <div class="col-6 col-sm-4">
                            <button type="button" class="slot-btn ${extraClass}" data-id="${slot.poolSlotId}" data-time="${slot.startTime.substring(0,5)} - ${slot.endTime.substring(0,5)}" data-full="${isFull}" data-late="${isLate}" data-inwaitlist="${slot.isInWaitlist}" data-waitlistid="${slot.waitlistEntryId || ''}" ${isDisabled}>
                                <div class="fs-5 fw-bold">${slot.startTime.substring(0,5)}</div>
                                <div class="small">${slot.endTime.substring(0,5)}</div>
                                <div class="small text-muted mt-1" style="font-size: 0.75rem;">${statusText}</div>
                            </button>
                        </div>
                    `;
                });
                
                $('#slots-container').html(html);

                // Gắn sự kiện click
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

    function loadTickets() {
        $.ajax({
            url: `${API_BASE_URL}/api/customer-bookings/pools/${POOL_ID}/tickets`,
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

                // Gắn sự kiện tăng giảm
                $('.qty-minus').on('click', function () {
                    const id = $(this).data('id');
                    const input = $(`#qty-${id}`);
                    let val = parseInt(input.val());
                    if (val > 0) {
                        input.val(val - 1);
                        calculateTotal();
                    }
                });

                $('.qty-plus').on('click', function () {
                    const id = $(this).data('id');
                    const slotEq = parseInt($(this).data('sloteq') || 1);
                    const input = $(`#qty-${id}`);
                    let val = parseInt(input.val());
                    
                    // Check total slots globally
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

    function updateSummaryTime() {
        if (selectedSlotTime) {
            $('#summary-time').text(selectedSlotTime).removeClass('text-muted').addClass('text-dark');
        } else {
            $('#summary-time').text('Chưa chọn').removeClass('text-dark').addClass('text-muted');
        }
    }

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

    // Gửi yêu cầu đặt lịch
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

        if (isSelectedSlotInWaitlist) {
            // Cancel Waitlist Logic
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
                    btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

                    $.ajax({
                        url: `${API_BASE_URL}/api/customer-bookings/waitlist/cancel`,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ waitlistEntryId: selectedWaitlistEntryId }),
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
                            let msg = "Đã xảy ra lỗi khi hủy danh sách chờ.";
                            if (xhr.responseJSON && xhr.responseJSON.message) {
                                msg = xhr.responseJSON.message;
                            }
                            Swal.fire({
                                icon: 'error',
                                title: 'Lỗi',
                                text: msg,
                                confirmButtonColor: '#0ea5e9'
                            });
                        }
                    });
                }
            });
        } else if (isSelectedSlotFull) {
            // Join Waitlist logic
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

                    btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

                    $.ajax({
                        url: `${API_BASE_URL}/api/customer-bookings/waitlist/join`,
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
                                    title: 'Lỗi',
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
                            let msg = "Đã xảy ra lỗi khi tham gia danh sách chờ.";
                            if (xhr.responseJSON && xhr.responseJSON.message) {
                                msg = xhr.responseJSON.message;
                            } else if (xhr.responseJSON && xhr.responseJSON.errors) {
                                msg = Object.values(xhr.responseJSON.errors).join('\n');
                            }
                            Swal.fire({
                                icon: 'error',
                                title: 'Lỗi',
                                text: msg,
                                confirmButtonColor: '#0ea5e9'
                            });
                        }
                    });
                }
            });
        } else {
            // Normal Booking logic
            const payload = {
                poolSlotId: selectedSlotId,
                tickets: tickets
            };

            btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

            $.ajax({
                url: `${API_BASE_URL}/api/customer-bookings/create`,
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
                    let msg = "Đã xảy ra lỗi khi tạo đặt lịch. Vui lòng thử lại.";
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        msg = xhr.responseJSON.message;
                    } else if (xhr.responseJSON && xhr.responseJSON.errors) {
                        msg = Object.values(xhr.responseJSON.errors).join('\n');
                    }
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: msg,
                        confirmButtonColor: '#0ea5e9'
                    });
                }
            });
        }
    });

});
