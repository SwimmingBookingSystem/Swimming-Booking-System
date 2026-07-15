document.addEventListener("DOMContentLoaded", function () {
    loadContactHistory(1);
});

// Hàm hỗ trợ lấy cookie, tuân thủ AGENTS.md
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

// Hàm chống XSS khi hiển thị dữ liệu
function escapeHtml(unsafe) {
    if (!unsafe) return '';
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/"/g, "&quot;")
         .replace(/'/g, "&#039;");
}

function loadContactHistory(pageNumber) {
    const token = getCookie('accessToken') || '';
    const pageSize = 10;
    
    Swal.fire({
        title: 'Đang tải dữ liệu...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    fetch(`${API_BASE_URL}/api/contacts/my-history?pageNumber=${pageNumber}&pageSize=${pageSize}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    })
    .then(response => {
        if (response.status === 401 || response.status === 403) {
            throw new Error('Unauthorized');
        }
        if (!response.ok) {
            throw new Error('FetchError');
        }
        return response.json();
    })
    .then(data => {
        Swal.close();
        renderContactHistory(data.items);
        
        // Kiểm tra logic theo chuẩn AGENTS.md (items.length > 0)
        if (data.items && data.items.length > 0) {
            renderPagination(data.page, data.totalPages);
        } else {
            document.getElementById('paginationContainer').innerHTML = '';
        }
    })
    .catch(error => {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: error.message === 'Unauthorized' ? 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.' : 'Đã có lỗi xảy ra khi tải lịch sử yêu cầu liên hệ.'
        });
    });
}

function renderContactHistory(items) {
    const container = document.getElementById('contactHistoryContainer');
    container.innerHTML = '';

    if (!items || items.length === 0) {
        container.innerHTML = '<div class="alert alert-info m-3">Bạn chưa gửi yêu cầu liên hệ nào trong hệ thống.</div>';
        return;
    }

    items.forEach(item => {
        const date = new Date(item.createdAt).toLocaleString('vi-VN');
        const handledDate = item.handledAt ? new Date(item.handledAt).toLocaleString('vi-VN') : '';
        
        let statusBadge = '';
        if (item.status === 'Pending') {
            statusBadge = '<span class="badge badge-ocean-warning">Đang chờ</span>';
        } else if (item.status === 'Resolved' || item.status === 'Completed' || item.status === 'Handled') {
            statusBadge = '<span class="badge badge-ocean-success">Đã xử lý</span>';
        } else {
            statusBadge = `<span class="badge badge-secondary">${item.status}</span>`;
        }

        const categoryEscaped = escapeHtml(item.category);
        const messageEscaped = escapeHtml(item.message);

        const html = `
            <div class="card mb-3 ocean-card shadow-sm border-0">
                <div class="card-header bg-white border-bottom-0 d-flex justify-content-between align-items-center" 
                     data-bs-toggle="collapse" 
                     data-bs-target="#collapse-${item.contactRequestId}" 
                     aria-expanded="false" 
                     style="cursor: pointer;">
                    <div>
                        <h6 class="mb-1 text-teal fw-bold">${categoryEscaped}</h6>
                        <small class="text-muted"><i class="bi bi-clock me-1"></i> ${date}</small>
                    </div>
                    <div class="d-flex align-items-center">
                        ${statusBadge}
                        <i class="bi bi-chevron-down ms-3 text-muted"></i>
                    </div>
                </div>
                <div id="collapse-${item.contactRequestId}" class="collapse" data-bs-parent="#contactHistoryContainer">
                    <div class="card-body bg-light border-top">
                        <p class="mb-2"><strong>Nội dung chi tiết:</strong></p>
                        <p class="mb-0 text-dark" style="white-space: pre-line;">${messageEscaped}</p>
                        ${item.handledAt ? `<hr><small class="text-success fw-bold"><i class="bi bi-check-circle me-1"></i> Đã phản hồi/xử lý lúc: ${handledDate}</small>` : ''}
                    </div>
                </div>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', html);
    });
}

function renderPagination(currentPage, totalPages) {
    const container = document.getElementById('paginationContainer');
    container.innerHTML = '';

    if (totalPages <= 1) return;

    let html = '<ul class="pagination mb-0">';
    
    html += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <button class="page-link" onclick="loadContactHistory(${currentPage - 1})">Trước</button>
             </li>`;

    for (let i = 1; i <= totalPages; i++) {
        html += `<li class="page-item ${currentPage === i ? 'active' : ''}">
                    <button class="page-link" onclick="loadContactHistory(${i})">${i}</button>
                 </li>`;
    }

    html += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <button class="page-link" onclick="loadContactHistory(${currentPage + 1})">Sau</button>
             </li>`;

    html += '</ul>';
    container.innerHTML = html;
}
