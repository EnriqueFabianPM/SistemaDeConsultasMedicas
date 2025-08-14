window.showSweetAlert = (title, text, icon, confirmButtonText = "Aceptar") => {
    Swal.fire({
        title: title,
        text: text,
        icon: icon, // 'success', 'error', 'warning', 'info', 'question'
        confirmButtonText: confirmButtonText
    });
};