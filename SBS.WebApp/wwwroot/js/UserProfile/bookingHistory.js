
// Logic for displaying QR Code from QrCodeData string and Pagination

document.addEventListener("DOMContentLoaded", function () {
    // --- QR Code Modal Logic ---
    const qrModal = document.getElementById('qrModal');
    if (qrModal) {
        qrModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const qrData = button.getAttribute('data-bs-qr');
            const bookingCode = button.getAttribute('data-bs-code');
            const poolName = button.getAttribute('data-bs-pool');
            const timeInfo = button.getAttribute('data-bs-time');
            const ticketsInfo = button.getAttribute('data-bs-tickets');

            const modalTitle = qrModal.querySelector('.modal-title');
            const qrContainer = document.getElementById('qrcode-container');
            const displayCode = document.getElementById('qr-booking-code-display');

            modalTitle.textContent = 'Mã QR Check-in';
            if (displayCode) displayCode.textContent = bookingCode;
            
            document.getElementById('qr-pool-name').textContent = poolName || '';
            document.getElementById('qr-time').textContent = timeInfo || '';
            document.getElementById('qr-tickets').textContent = ticketsInfo || '';

            qrContainer.innerHTML = '';

            if (qrData && typeof QRCode !== 'undefined') {
                new QRCode(qrContainer, {
                    text: qrData,
                    width: 170, // Slightly smaller to fit the nice border
                    height: 170,
                    colorDark: "#005F6A", // Ocean Dark Teal
                    colorLight: "#ffffff",
                    correctLevel: QRCode.CorrectLevel.H
                });
            } else {
                qrContainer.innerHTML = '<p class="text-danger mt-4">Không thể tải mã QR hoặc thiếu thư viện qrcode.js</p>';
            }
        });
    }

    // --- Pagination Logic ---
    const itemsPerPage = 5;
    const items = document.querySelectorAll('.booking-item');
    const paginationContainer = document.getElementById('booking-pagination');
    const paginationUl = document.getElementById('pagination-ul');

    if (items.length > 0) {
        paginationContainer.style.display = 'block';
        const pageCount = Math.ceil(items.length / itemsPerPage);
        let currentPage = 1;

        function showPage(page) {
            items.forEach((item, index) => {
                item.style.display = (index >= (page - 1) * itemsPerPage && index < page * itemsPerPage) ? 'block' : 'none';
            });
            updatePaginationUI(page);
        }

        function updatePaginationUI(activePage) {
            paginationUl.innerHTML = '';

            const createLi = (text, targetPage, isDisabled, isActive = false) => {
                const li = document.createElement('li');
                li.className = `page-item ${isActive ? 'active' : ''} ${isDisabled ? 'disabled' : ''}`;
                li.innerHTML = `<a class="page-link" href="javascript:void(0)">${text}</a>`;
                if (!isDisabled && !isActive) {
                    li.addEventListener('click', function (e) {
                        e.preventDefault();
                        currentPage = targetPage;
                        showPage(currentPage);
                        document.getElementById('booking-list-container').scrollIntoView({ behavior: 'smooth', block: 'start' });
                    });
                }
                return li;
            };

            // Đầu & Trước
            paginationUl.appendChild(createLi('Đầu', 1, activePage === 1));
            paginationUl.appendChild(createLi('Trước', activePage - 1, activePage === 1));

            // Numbers
            for (let i = 1; i <= pageCount; i++) {
                paginationUl.appendChild(createLi(i, i, false, i === activePage));
            }

            // Sau & Cuối
            paginationUl.appendChild(createLi('Sau', activePage + 1, activePage === pageCount));
            paginationUl.appendChild(createLi('Cuối', pageCount, activePage === pageCount));
        }

        // Initialize first page
        showPage(currentPage);
    }
});
