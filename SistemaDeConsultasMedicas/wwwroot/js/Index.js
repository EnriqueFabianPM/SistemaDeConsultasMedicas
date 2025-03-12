const { createApp } = Vue;

const app = createApp({
    data() {
        return {
            Authorization: {
                Success: false,
                User: null,
            },

            user: window.user,
        };
    },
    methods: {
        //Aquí se crearán los métodos js
        createUser() {

            axios.post(createUser, {

            })
                .then(response => {

                    console.log(response.data, 'respuesta del método');
                })
                .catch(error => {
                    console.log(error, 'Mensaje de error')
                });
        },
    },
    mounted() {
        console.log(window.user); // Ahora es un objeto JSON usable
        if (window.user.id_User) {
            this.Authorization = {
                Success: true,
                User: null,
            };
        }
    }
});
app.mount('#app');