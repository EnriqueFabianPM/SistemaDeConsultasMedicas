const appLayout = Vue.createApp({
    data() {
        return {
            //Modelo para manejar autorización y almacenar datos del usuario
            Authorization: {
                Success: false,
                User: null,
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
        console.log(`user: ${window.user}`);
        if (window.user) {
            this.Authorization = {
                Success: true,
                User: null,
            };

            this.user = window.user;
        }
    }
});
appLayout.mount('#layout');