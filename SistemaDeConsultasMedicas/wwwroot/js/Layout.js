const { createApp } = Vue;

const app = createApp({
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
    mounted() {
        console.log(window.user);
        if (window.user) {
            this.Authorization = {
                Success: true,
                User: null,
            };
        }
    }
});
app.mount('#app');