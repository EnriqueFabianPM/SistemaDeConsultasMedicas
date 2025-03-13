const { createApp } = Vue;

// Si no, asegúrate de incluir axios via CDN en el HTML:
// <script src="https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js"></script>

// Datos y lógica de Vue.js
const app = new Vue({
    el: '#app',
    data: {
        users: [] // Inicialmente vacío
    },
    mounted() {
        // Llamar a la función para obtener datos del backend
        this.fetchUsers();
    },
    methods: {
        // Función para obtener datos del backend
        fetchUsers() {
            axios.get('http://localhost:3000/api/users') // Cambia la URL por la de tu backend
                .then(response => {
                    this.users = response.data; // Asignar los datos a la variable users
                    this.initializeDataTable(); // Inicializar DataTables después de obtener los datos
                })
                .catch(error => {
                    console.error('Error al obtener los datos:', error);
                    alert('No se pudieron cargar los datos. Por favor, inténtalo de nuevo.');
                });
        },
        // Función para inicializar DataTables
        initializeDataTable() {
            $('#users-table').DataTable({
                paging: true, // Habilitar paginación
                searching: true, // Habilitar búsqueda
                ordering: true, // Habilitar ordenación
                info: true, // Mostrar información de paginación
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json' // Español
                }
            });
        }
    }
});