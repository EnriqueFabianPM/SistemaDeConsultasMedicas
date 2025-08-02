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

            form: {
                Id_User: user.id_User,
                Name: "",
                Email: "",
                Phone: "",
                fk_Role: 0,
                Active: false,
                municipality: '',
                fk_Consultory: '',
                fk_Type: '',
            },

            //Objeto donde se pondrá la configuración para buscar la API y parámetros que recibe
            config: {
                IdApi: null, //Id de la API (Base de datos)
                BodyParams: null, //Objeto (Generalmente para métodos tipo "Post")
                Param: null, //Objeto exclusivo para ÁPIs tipo "Get"
            },

            user: {},
            users: [],
            roles: [],
            types: [],
            consultories: [],
            municipalities: [],
        };
    },
    methods: {
        getRoles() {
            this.config.IdApi = 17;

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Lista de roles', response.data);
                    this.roles = response.data;
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        getTypes() {
            this.config.IdApi = 16;

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Lista de types', response.data);
                    this.types = response.data;
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
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

        getMunicipalities() {
            //Id para obtener los municipios
            this.config.IdApi = 1;

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Municipios', response.data);
                    this.municipalities = response.data;
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        //Obtener los consultorios
        getConsultories(id) {
            this.config = {
                IdApi: 2, //Consultorios
                BodyParams: null, //Al ser una api tipo Get se manda null
                Param: `${id}`, //Se transforma el valor en string
            };

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Consultorios', response.data);
                    this.consultories = response.data;
                    this.form.fk_Consultory = response.data.find(c => c.id === this.form.fk_Consultory)?.id || '';
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
                            window.location.reload();
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

        chargeFields(user) {
            console.log('Usuario a editar: ', user);
            console.log('form: ', this.form);
            this.form.fk_Consultory = 0;
            this.form.municipality = '';
            this.form.fk_Type = '';

            this.form.Id_User = user.id_User;
            this.form.Name = user.name;
            this.form.Email = user.email;
            this.form.Phone = user.phone === "-" ? "" : user.phone;
            this.form.fk_Role = user.fk_Role;
            this.form.Active = user.active === "Activo" ? true : false;
            this.form.fk_Consultory = user.fk_Consultory !== null ? user.fk_Consultory : 0;
            this.form.municipality = user.fk_Consultory !== null ? this.municipalities.find(m => m.id === user.fk_Municipality)?.id : '';
            this.form.fk_Type = user.fk_Type !== null ? this.types.find(t => t.id === user.fk_Type)?.id : '';

            if (this.form.fk_Consultory != 0) this.getConsultories(user.fk_Municipality);
        },

        onMunicipalityChange() {
            this.getConsultories(this.form.municipality);    
        },

        updateUser() {

            if (this.form.fk_Role === 3 &&
                this.form.fk_Consultory === '' ||
                this.form.fk_Type === ''
            ) {
                Swal.fire({
                    title: "Error",
                    text: "Favor de añadír la informacion faltante.",
                    icon: "error",
                    confirmButtonText: "Aceptar",
                });
                return;
            }

            Swal.fire({
                title: "Actualizando usuario...",
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
                    Id_User: this.form.Id_User,
                    Name: this.form.Name,
                    Email: this.form.Email,
                    Phone: this.form.Phone === "-" ? null : this.form.Phone,
                    Active: this.form.Active,
                    fk_Role: this.form.fk_Role,
                    fk_Consultory: this.form.fk_Consultory,
                    fk_Type: this.form.fk_Type,
                },
                Param: null,
            };

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
                            window.location.reload();
                        });
                    } else {
                        Swal.fire({
                            title: "Error",
                            text: "No se ha podido actualizar el usuario",
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
        this.getRoles();
        this.getTypes();
        this.getMunicipalities();
    },
    beforeDestroy() {
        // Destruye la instancia de DataTables cuando el componente se destruya para evitar fugas de memoria
        $(this.$refs.userTable).DataTable().destroy();
    }
});
app.mount('#app');