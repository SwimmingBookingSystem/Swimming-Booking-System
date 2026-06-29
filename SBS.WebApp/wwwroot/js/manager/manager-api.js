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
            
            // Cấu hình Header, tự động đính kèm Token nếu có
            const headers = {
                'Content-Type': 'application/json'
            };
            let token = localStorage.getItem('manager_token');
            if (!token) {
                // Auto dev login (Bypass for testing without Login UI)
                $.ajax({
                    url: (window.API_BASE_URL || 'https://localhost:7179') + '/api/Auth/login',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ userName: "manager1", password: "Password@123" }),
                    async: false, // Wait for token
                    success: function(res) {
                        token = res.accessToken || res.token || res;
                        localStorage.setItem('manager_token', token);
                    },
                    error: function(xhr) {
                        console.error('Auto login failed:', xhr);
                        alert('Auto login failed. Check console for details. Make sure SBS.Api is running on port 7179.');
                    }
                });
            }
            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            } else {
                $('#global-loader').hide();
                reject(new Error("No token available"));
                return;
            }

            // Sử dụng jQuery $.ajax
            $.ajax({
                url: this.baseURL + url,
                type: method,
                headers: headers,
                data: data ? JSON.stringify(data) : null,
                success: function(response) {
                    $('#global-loader').hide();
                    // jQuery tự động parse JSON nên response đã là object
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
                        toastr.error('Phiên đăng nhập tự động đã làm mới. Vui lòng thử lại.');
                        localStorage.removeItem('manager_token');
                        setTimeout(() => window.location.reload(), 1000);
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
    delete: function(url) { return this.request('DELETE', url); }
};
