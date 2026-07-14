// UserProfile/profile.js
// Logic for User Profile tab interactions and form handling if needed.

document.addEventListener("DOMContentLoaded", function () {
    // 1. Handle Avatar image preview before upload
    const avatarInput = document.getElementById("AvatarFile");
    const avatarPreview = document.getElementById("avatarPreview");

    if (avatarInput && avatarPreview) {
        avatarInput.addEventListener("change", function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    avatarPreview.src = e.target.result;
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // 2. Add other UI enhancements for Profile here
    console.log("Profile JS loaded.");
});
