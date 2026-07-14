// UserProfile/bookingHistory.js
// Logic for displaying QR Code from QrCodeData string

document.addEventListener("DOMContentLoaded", function () {
    const qrModal = document.getElementById('qrModal');
    if (!qrModal) return;

    // Listen for modal show event (Bootstrap 5)
    qrModal.addEventListener('show.bs.modal', function (event) {
        // Button that triggered the modal
        const button = event.relatedTarget;
        // Extract info from data-bs-* attributes
        const qrData = button.getAttribute('data-bs-qr');
        const bookingCode = button.getAttribute('data-bs-code');

        // Update the modal's content
        const modalTitle = qrModal.querySelector('.modal-title');
        const qrContainer = document.getElementById('qrcode-container');

        modalTitle.textContent = 'Mã QR Check-in: ' + bookingCode;
        
        // Clear previous QR code if any
        qrContainer.innerHTML = '';

        if (qrData && typeof QRCode !== 'undefined') {
            // Render the new QR Code
            new QRCode(qrContainer, {
                text: qrData,
                width: 256,
                height: 256,
                colorDark : "#000000",
                colorLight : "#ffffff",
                correctLevel : QRCode.CorrectLevel.H
            });
        } else {
            qrContainer.innerHTML = '<p class="text-danger">Không thể tải mã QR hoặc thiếu thư viện qrcode.js</p>';
        }
    });
});
