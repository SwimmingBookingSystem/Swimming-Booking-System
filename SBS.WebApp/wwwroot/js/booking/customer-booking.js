$(document).ready(function () {
    let selectedSlotId = null;
    let selectedSlotTime = null;
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
        updateSummaryTime();
        checkSubmitEnable();

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
                    const slotDateTime = new Date(slotDateTimeStr);
                    const isPassed = slotDateTime <= now;

                    const isDisabled = (slot.availableCapacity <= 0 || isPassed) ? 'disabled' : '';
                    
                    let statusText = `(Còn ${slot.availableCapacity} / ${slot.capacity})`;
                    if (isPassed) {
                        statusText = '(Đã qua)';
                    } else if (slot.availableCapacity <= 0) {
                        statusText = '(Hết chỗ)';
                    }
                    
                    html += `
                        <div class="col-6 col-sm-4">
                            <button type="button" class="slot-btn" data-id="${slot.poolSlotId}" data-time="${slot.startTime.substring(0,5)} - ${slot.endTime.substring(0,5)}" ${isDisabled}>
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
                    updateSummaryTime();
                    checkSubmitEnable();
                });
            },
            error: function () {
                $('#slots-loader').addClass('d-none');
                $('#slots-error').removeClass('d-none').text('Đã xảy ra lỗi khi tải danh sách giờ. Vui lòng kiểm tra lại kết nối hoặc đăng nhập.');
            }
        });
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

                let html = '';
                tickets.forEach(ticket => {
                    html += `
                        <div class="ticket-card d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="fw-bold mb-1">${ticket.ticketName}</h6>
                                <div class="text-primary fw-semibold">${Number(ticket.price).toLocaleString('vi-VN')}đ</div>
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <button type="button" class="qty-btn qty-minus" data-id="${ticket.poolTicketTypeId}">-</button>
                                <input type="number" class="qty-input" id="qty-${ticket.poolTicketTypeId}" value="0" min="0" max="10" readonly>
                                <button type="button" class="qty-btn qty-plus" data-id="${ticket.poolTicketTypeId}">+</button>
                            </div>
                        </div>
                    `;
                });

                $('#tickets-container').html(html);

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
                    const input = $(`#qty-${id}`);
                    let val = parseInt(input.val());
                    if (val < 10) {
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
        let hasTicket = false;
        ticketsData.forEach(ticket => {
            if (parseInt($(`#qty-${ticket.poolTicketTypeId}`).val() || 0) > 0) {
                hasTicket = true;
            }
        });

        if (selectedSlotId && hasTicket) {
            $('#btn-submit-booking').prop('disabled', false);
        } else {
            $('#btn-submit-booking').prop('disabled', true);
        }
    }

    // Gửi yêu cầu đặt lịch
    $('#btn-submit-booking').on('click', function () {
        const btn = $(this);
        
        // Build request body
        const tickets = [];
        ticketsData.forEach(ticket => {
            const qty = parseInt($(`#qty-${ticket.poolTicketTypeId}`).val() || 0);
            if (qty > 0) {
                tickets.push({
                    poolTicketTypeId: ticket.poolTicketTypeId,
                    quantity: qty
                });
            }
        });

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
                // response: { bookingId, bookingCode, paymentLink }
                if (response.paymentLink) {
                    window.location.href = response.paymentLink;
                } else {
                    alert("Đặt lịch thành công nhưng không tìm thấy link thanh toán.");
                    btn.html('Đặt lịch ngay <i class="bi bi-arrow-right-circle ms-1"></i>');
                }
            },
            error: function (xhr) {
                btn.prop('disabled', false).html('Đặt lịch ngay <i class="bi bi-arrow-right-circle ms-1"></i>');
                console.error("Booking error:", xhr);
                let msg = "Đã xảy ra lỗi khi tạo đặt lịch. Vui lòng thử lại.";
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg = xhr.responseJSON.message;
                }
                alert(msg);
            }
        });
    });

});
