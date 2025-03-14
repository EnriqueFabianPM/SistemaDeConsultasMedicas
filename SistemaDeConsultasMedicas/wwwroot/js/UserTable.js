document.addEventListener("DOMContentLoaded", function () {
    const users = [
        { id: 1, nombre: 'Juan Pérez', correo: 'juan@example.com', status: 'Aprobada' },
        { id: 2, nombre: 'Ana López', correo: 'ana@example.com', status: 'En proceso' }
    ];

    const tableBody = document.getElementById("userTableBody");

    users.forEach(user => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${user.id}</td>
            <td>${user.nombre}</td>
            <td>${user.correo}</td>
            <td>${user.status}</td>
            <td class="actions">
                <button class="edit-btn" onclick="editUser(${user.id})">Editar</button>
                <button class="delete-btn" onclick="deleteUser(${user.id})">Borrar</button>
            </td>
        `;
        tableBody.appendChild(row);
    });
});

function editUser(id) {
    alert("Editar usuario con ID: " + id);
}

function deleteUser(id) {
    if (confirm("¿Seguro que quieres eliminar este usuario?")) {
        alert("Usuario con ID " + id + " eliminado.");
    }
}
