const { createApp } = Vue;

const app = createApp({
    data() {
        return {

            //Modelo para credenciales del usuario
            credentials: {
                Email: "",
                Password: "",
            },

            //Modelo para manejar autorización y almacenar datos del usuario
            Authorization: {
                Success: false,
                User: null,
            },

            //Modelo para nuevos usuarios
            newUser: {
                id_User: null,
                name: "",
                email: "",
                password: "",
                phone: null,
                fk_Sex: null,
                fk_Role: null,
                fk_Consultory: null,
                fk_Type: null,
                fk_Schedule: null,
                active: false,
                sex: "",
                role: "",
                consultory: "",
                type: "",
                schedule: "",
            },

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

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Usuario', response.data);

                    if (response.data.id_User != 0) {

                        this.goToIndex(response.data.id_User);

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
                            email: "",
                            password: "",
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

        goToIndex(idUser) {
            window.location.href = `${window.Index}?id=${idUser}`;
        }

    },
    mounted() {

    }
});
app.mount('#app');