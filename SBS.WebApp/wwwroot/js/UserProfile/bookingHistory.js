
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
});
