document.addEventListener("DOMContentLoaded", function () {
    const avatarInput = document.getElementById("avatarInput");
    const avatarPreview = document.getElementById("avatarPreview");
    const headerAvatar = document.getElementById("header-user-avatar");

    if (avatarInput) {
        avatarInput.addEventListener("change", function () {
            const file = this.files[0];
            if (!file) return;

            // 1. Kiểm tra định dạng file ảnh
            if (!file.type.startsWith('image/')) {
                Swal.fire({
                    icon: 'error',
                    title: 'Định dạng không hợp lệ',
                    text: 'Vui lòng chọn một tệp hình ảnh (JPEG, PNG, WEBP...)',
                    customClass: { popup: 'ocean-swal-popup' },
                    confirmButtonColor: '#00C2E0'
                });
                return;
            }

            // 2. Kiểm tra dung lượng (tối đa 5MB)
            if (file.size > 5 * 1024 * 1024) {
                Swal.fire({
                    icon: 'warning',
                    title: 'File quá lớn',
                    text: 'Dung lượng ảnh đại diện không được vượt quá 5MB.',
                    customClass: { popup: 'ocean-swal-popup' },
                    confirmButtonColor: '#00C2E0'
                });
                return;
            }

            const formData = new FormData();
            formData.append('file', file);

            // 3. Hiển thị Loading Popup theo chuẩn Ocean Theme
            Swal.fire({
                title: 'Đang tải ảnh đại diện lên...',
                text: 'Hệ thống đang xử lý và lưu trữ ảnh trên đám mây',
                customClass: { popup: 'ocean-swal-popup' },
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            const apiBaseUrl = window.API_BASE_URL || 'https://localhost:7179';
            const token = window.ACCESS_TOKEN || '';

            // 4. Gọi API Upload Avatar lên Backend
            $.ajax({
                url: `${apiBaseUrl}/api/profile/upload-avatar`,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                headers: {
                    'Authorization': 'Bearer ' + token
                },
                xhrFields: {
                    withCredentials: true
                },
                success: function (response) {
                    if (response && response.avatarUrl) {
                        const newAvatarUrl = response.avatarUrl;

                        // a. Cập nhật giao diện HTML trên client tức thời
                        if (avatarPreview) {
                            avatarPreview.src = newAvatarUrl;
                        }
                        if (headerAvatar) {
                            headerAvatar.src = newAvatarUrl;
                        }

                        // b. Đồng bộ Claim "AvatarUrl" vào Cookie của WebApp Razor Page
                        const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
                        $.ajax({
                            url: '/Customer/Profile/Index?handler=SyncAvatarClaim',
                            type: 'POST',
                            contentType: 'application/json',
                            data: JSON.stringify({ avatarUrl: newAvatarUrl }),
                            headers: {
                                'RequestVerificationToken': antiForgeryToken
                            },
                            complete: function () {
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Cập nhật thành công!',
                                    text: 'Ảnh đại diện của bạn đã được tải lên và đồng bộ toàn hệ thống.',
                                    customClass: { popup: 'ocean-swal-popup' },
                                    confirmButtonColor: '#00C2E0'
                                });
                            }
                        });
                    }
                },
                error: function (xhr) {
                    Swal.close();
                    let msg = "Đã xảy ra lỗi khi tải ảnh đại diện lên server.";
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        msg = xhr.responseJSON.message;
                    }
                    Swal.fire({
                        icon: 'error',
                        title: 'Tải ảnh thất bại',
                        text: msg,
                        customClass: { popup: 'ocean-swal-popup' },
                        confirmButtonColor: '#005F6A'
                    });
                }
            });
        });
    }
});
