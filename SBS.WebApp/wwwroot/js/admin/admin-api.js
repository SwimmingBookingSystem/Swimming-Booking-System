// Setup Toastr
toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "3000",
};

const api = {
    baseURL: (window.API_BASE_URL || 'https://localhost:7179') + '/api/admin',
    
    request: function(method, url, data) {
        return new Promise((resolve, reject) => {
            $('#global-loader').css('display', 'flex');
            
            const token = window.ACCESS_TOKEN;

            const headers = {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            };

            $.ajax({
                url: this.baseURL + url,
                type: method,
                headers: headers,
                xhrFields: {
                    withCredentials: true
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
                    
                    if (status === 401) {
                        toastr.error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                        setTimeout(() => {
                            window.location.href = '/Auth/Logout';
                        }, 1500);
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
    
    get: function(url) { return this.request('GET', url); },
    post: function(url, data) { return this.request('POST', url, data); },
    put: function(url, data) { return this.request('PUT', url, data); },
    patch: function(url, data) { return this.request('PATCH', url, data); },
    delete: function(url) { return this.request('DELETE', url); }
};
