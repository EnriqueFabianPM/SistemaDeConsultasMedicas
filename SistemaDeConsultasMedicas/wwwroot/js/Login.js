const app = Vue.createApp({
    data() {
        return {
            isLogin: true,
            
            //Modelo para credenciales del usuario
            credentials: {
                Email: '',
                Password: '',
            },

            //Modelo para manejar autorización y almacenar datos del usuario
            Authorization: {
                Success: false,
                User: null,
            },

            //Modelo para nuevos usuarios
            newUser: {
                id_User: 0,
                name: '',
                email: '',
                password: '',
                phone: '',
                fk_Sex: '',
                fk_Role: 2,
            },

            confirmPassword: '',

            //Objeto donde se pondrá la configuración para buscar la API y parámetros que recibe
            config: {
                IdApi: null, //Id de la API (Base de datos)
                BodyParams: null, //Objeto (Generalmente para métodos tipo "Post")
                Param: null, //Objeto exclusivo para ÁPIs tipo "Get"
            }
        };
    },
    methods: {
        //Obtener Autorización
        login() {
            this.config = {
                IdApi: 9,
                BodyParams: this.credentials,
                Param: null,
            };

            Swal.fire({
                title: "Iniciando sesión...",
                text: "Por favor, espera",
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Usuario', response.data);

                    if (response.data.id_User != 0) {

                        Swal.fire({
                            title: "Éxito",
                            text: "Inicio de sesión exitoso",
                            icon: "success",
                            timer: 1500,
                            allowOutsideClick: false,
                            showConfirmButton: false
                        }).then(() => {
                            this.goToIndex(response.data.id_User);
                        })

                    } else {
                        Swal.fire({
                            title: "Error",
                            text: "Las credenciales son incorrectas",
                            icon: "error",
                            timer: 1000,
                            showConfirmButton: false,
                        });

                        //Modelo para credenciales del usuario
                        this.credentials = {
                            email: '',
                            password: '',
                        };

                        //Modelo para manejar autorización y almacenar datos del usuario
                        this.Authorization = {
                            Success: false,
                            User: null,
                        };
                    }
                    console.log('Autorización', this.Authorization);

                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        register() {
            if (this.newUser.password === this.confirmPassword) {

                Swal.fire({
                    title: "Error",
                    text: "Las contraseñas son diferentes",
                    icon: "error",
                    timer: 1500,
                    showConfirmButton: false
                });
                return;
            }

            this.config = {
                IdApi: 11, // Cambiar según el Id de tu API para registrar
                BodyParams: this.newUser,
                Param: null,
            };

            console.log('Registrando usuario', this.newUser);

            Swal.fire({
                title: "Registrando usuario...",
                text: "Por favor, espera",
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Respuesta de registro', response.data);

                    if (response.data.success) {
                        Swal.fire({
                            title: "Éxito",
                            text: response.data.message || "Usuario registrado correctamente",
                            icon: "success",
                            timer: 2000,
                            allowOutsideClick: false,
                            showConfirmButton: false
                        }).then(() => {
                            // Puedes redirigir o pasar automáticamente a login
                            this.isLogin = true;
                            // Limpiar campos del formulario
                            this.newUser = {
                                id_User: 0,
                                name: '',
                                email: '',
                                password: '',
                                phone: '',
                                fk_Sex: '',
                                fk_Role: 2,
                            };
                        });
                    } else {
                        Swal.fire({
                            title: "Error",
                            text: response.data.message || "No se pudo registrar el usuario",
                            icon: "error",
                            timer: 1500,
                            showConfirmButton: false
                        });
                    }
                })
                .catch(error => {
                    console.error("Error en la petición de registro:", error);
                    Swal.fire({
                        title: "Error",
                        text: "Hubo un error al registrar el usuario",
                        icon: "error",
                        timer: 1500,
                        showConfirmButton: false
                    });
                });
        },

        goToIndex(idUser) {
            const root = window.index || null;
            if (root) window.location.href = `${root}?id=${idUser}`;
        }
    },
    mounted() {
    } 
});
app.mount('#app');