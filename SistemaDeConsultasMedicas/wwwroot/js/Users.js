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
                    console.log('Lista de usuarios', response.data)
                    this.users = response.data;

                    this.$nextTick(() => {
                        // Destruir la instancia de DataTable si ya existe
                        if ($.fn.dataTable.isDataTable(this.$refs.userTable)) {
                            $(this.$refs.userTable).DataTable().destroy();
                        }

                        this.$nextTick(() => {
                            $(this.$refs.userTable).DataTable({
                                paging: true,
                                searching: true,
                                ordering: true,
                                responsive: true,
                                scrollY: '500px',
                                scrollCollapse: true,
                                language: {
                                    processing: "Procesando...",
                                    search: "Buscar:",
                                    lengthMenu: "Mostrar _MENU_ registros",
                                    info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
                                    infoEmpty: "No hay registros disponibles",
                                    infoFiltered: "(filtrado de _MAX_ registros en total)",
                                    loadingRecords: "Cargando...",
                                    zeroRecords: "No se encontraron registros",
                                    emptyTable: "No hay datos disponibles en la tabla",
                                    paginate: {
                                        first: "Primero",
                                        previous: "Anterior",
                                        next: "Siguiente",
                                        last: "Último"
                                    },
                                    aria: {
                                        sortAscending: ": activar para ordenar ascendente",
                                        sortDescending: ": activar para ordenar descendente"
                                    }
                                }
                            });
                        });
                    });
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        blockUser(user){
            console.log(`Usuario a bloquear`, user);

            Swal.fire({
                title: "Bloqueando usuario...",
                text: "Por favor, espera",
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            this.config = {
                IdApi: 7,
                BodyParams: {
                    Id_User: user.id_User,
                    Name: user.name,
                    Email: user.email,
                    Phone: user.phone === "-" ? null : user.phone,
                    Active: user.active === "Activo" ? false : true,
                },
                Param: null,
            }

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    if (response.data.success) {
                        Swal.fire({
                            title: "¡Listo!",
                            text: `${response.data.message}`,
                            icon: "success",
                            timer: 1500,
                            showConfirmButton: false,
                            allowOutsideClick: false,
                        }).then(() => {
                            this.Users(this.user.id_User);
                        })

                    } else {
                        Swal.fire({
                            title: "Error",
                            text: "No se ha podido bloquear el usuario",
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