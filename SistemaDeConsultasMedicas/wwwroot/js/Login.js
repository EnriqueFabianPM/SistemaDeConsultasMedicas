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


                        this.goToIndex(response.data);

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

        goToIndex(user) {
            this.Authorization = {
                Success: true,
                User: user,
            };

            fetch(window.goToIndex, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(this.Authorization),
            })
                .then(response => response.text()) // Leer respuesta como texto
                .then(html => {
                    document.open();
                    document.write(html); // Reemplaza el contenido de la página con la nueva vista
                    document.close();
                })
                .catch(error => console.error("Error en la autorización:", error));
        },
    },
    mounted() {

    }
});
app.mount('#app');