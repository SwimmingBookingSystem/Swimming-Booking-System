// Setup Toastr
toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "3000",
};

// Tạo một object bọc jQuery AJAX để sử dụng tương tự như Axios (hỗ trợ async/await)
const api = {
    baseURL: (window.API_BASE_URL || 'https://localhost:7179') + '/api/manager',
    
    request: function(method, url, data) {
        return new Promise((resolve, reject) => {
            $('#global-loader').css('display', 'flex');
            
            const token = window.ACCESS_TOKEN;
            if (!token) {
                $('#global-loader').hide();
                window.location.href = '/Auth/Logout';
                reject(new Error("No active session available"));
                return;
            }

            // Cấu hình Header công khai kèm Token mã hóa ở server
            const headers = {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            };

            // Sử dụng jQuery $.ajax
            $.ajax({
                url: this.baseURL + url,
                type: method,
                headers: headers,
                xhrFields: {
                    withCredentials: true // Tự động gửi kèm Cookie của API
                },
                data: data ? JSON.stringify(data) : null,
                success: function(response) {
                    $('#global-loader').hide();
                    resolve(response); 
                },
                error: function(xhr) {
                    $('#global-loader').hide();
                    console.error("API Error: ", xhr);
                    
                    const status = xhr.status;
                    let data = {};
                    try {
                        data = JSON.parse(xhr.responseText);
                    } catch(e) {}
                    
                    // Xử lý lỗi toàn cục
                    if (status === 401) {
                        toastr.error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                        setTimeout(() => window.location.href = '/Auth/Logout', 1000);
                    } else if (status === 403) {
                        toastr.error('Bạn không có quyền thực hiện thao tác này.');
                    } else if (status === 400 || status === 404 || status === 422) {
                        const errs = data.errors || data.Errors;
                        const msg = data.message || data.Message;
                        const title = data.title || data.Title;

                        if (errs && Array.isArray(errs) && errs.length > 0) {
                            errs.forEach(err => toastr.error(err));
                        } else if (errs && typeof errs === 'object') {
                            Object.values(errs).forEach(errArr => {
                                errArr.forEach(err => toastr.error(err));
                            });
                        } else if (msg) {
                            toastr.error(msg);
                        } else if (title) {
                            toastr.error(title);
                        } else {
                            toastr.error('Lỗi (' + status + '): ' + (xhr.responseText ? xhr.responseText.substring(0,100) : 'Yêu cầu không hợp lệ.'));
                        }
                    } else if (status === 500) {
                        toastr.error('Đã xảy ra lỗi hệ thống (500). Vui lòng thử lại sau.');
                    } else {
                        toastr.error('Lỗi máy chủ không xác định.');
                    }
                    
                    reject(xhr);
                }
            });
        });
    },
    
    // Các hàm tiện ích
    get: function(url) { return this.request('GET', url); },
    post: function(url, data) { return this.request('POST', url, data); },
    put: function(url, data) { return this.request('PUT', url, data); },
    patch: function(url, data) { return this.request('PATCH', url, data); },
    delete: function(url) { return this.request('DELETE', url); },
    
    uploadFile: function(url, file) {
        return new Promise((resolve, reject) => {
            $('#global-loader').css('display', 'flex');
            const formData = new FormData();
            formData.append('file', file);
            
            $.ajax({
                url: this.baseURL + url,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                xhrFields: {
                    withCredentials: true
                },
                success: function(response) {
                    $('#global-loader').hide();
                    resolve(response);
                },
                error: function(xhr) {
                    $('#global-loader').hide();
                    console.error("Upload Error: ", xhr);
                    toastr.error('Lỗi khi tải ảnh lên. Vui lòng thử lại.');
                    reject(xhr);
                }
            });
        });
    }
};
