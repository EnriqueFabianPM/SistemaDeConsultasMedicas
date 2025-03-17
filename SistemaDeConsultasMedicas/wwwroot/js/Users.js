const { createApp } = Vue;
const app = createApp({
    data() {
        return {
            Authorization: {
                Success: false,
                User: null,
            },

            //Modelo para credenciales del usuario
            credentials: {
                Email: "",
                Password: "",
            },

            users: [],

            //Objeto donde se pondrá la configuración para buscar la API y parámetros que recibe
            config: {
                IdApi: null, //Id de la API (Base de datos)
                BodyParams: null, //Objeto (Generalmente para métodos tipo "Post")
                Param: null, //Objeto exclusivo para ÁPIs tipo "Get"
            },

            user: {},
        };
    },
    methods: {
        Index(idUser) {
            window.location.href = `${window.index}?id=${idUser}`;
        },

        Appointments(idUser) {
            window.location.href = `${window.appointments}?id=${idUser}`;
        },

        Users(idUser) {
            window.location.href = `${window.users}?id=${idUser}`;
        },

        getUsers() {
            this.config.IdApi = 6;

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Lista de usuarios',response.data)
                    this.users = response.data;

                    // Destruir la instancia previa de DataTable antes de inicializarla nuevamente
                    this.$nextTick(() => {
                        // Destruir la instancia de DataTable si ya existe
                        if ($.fn.dataTable.isDataTable(this.$refs.userTable)) {
                            $(this.$refs.userTable).DataTable().destroy();
                        }

                        // Inicializar DataTable después de que Vue haya terminado de renderizar los datos
                        $(this.$refs.userTable).DataTable({
                            paging: true,
                            searching: true,
                            ordering: true,
                            responsive: true,
                            scrollY: '500px', // Esto habilita el scroll vertical a partir de 500px de altura
                            scrollCollapse: true, // Permite que la tabla colapse si no se necesita el scroll
                        });
                    });

                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });

        },

        //Aquí se crearán los métodos js
        logout() {
            this.credentials = {
                Email: user.email,
                Password: "",
            };

            this.config = {
                IdApi: 10,
                BodyParams: this.credentials,
                Param: null,
            };
            console.log('Se accedió a logout');

            axios.post(window.callApiAsync, this.config)
                .then(response => {

                    if (response.data.success) {
                        Swal.fire({
                            title: "¡Listo!",
                            text: `${response.data.message}`,
                            icon: "success",
                            timer: 1500,
                            showConfirmButton: false,
                        //    allowClickOutside: false,
                        }).then(() => {
                            window.location.href = window.login;
                        })

                    } else {
                        Swal.fire({
                            title: "Error",
                            text: "No se ha podido cerrar sesión",
                            icon: "error",
                            timer: 1500,
                            showConfirmButton: false,
                        });
                    }
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

    },
    mounted() {
        console.log(window.user, 'Usuario'); // Ahora es un objeto JSON usable
        if (window.user?.id_User) {
            this.Authorization = {
                Success: true,
                User: null,
            };

            this.user = window.user;
        }

        this.getUsers();
    },
    beforeDestroy() {
        // Destruye la instancia de DataTables cuando el componente se destruya para evitar fugas de memoria
        $(this.$refs.userTable).DataTable().destroy();
    }
});
app.mount('#app');