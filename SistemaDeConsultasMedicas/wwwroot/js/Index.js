const { index } = Vue;

const app = index({
    data() {
        return {

            /*
             Aquí vas a declarar los modelos de vue
             también hice ejemplos de cómo debes
             inicializar los modelos dependiendo el 
             tipo de cosa que quieras consumir de 
             una API.

             También para hacer modelos reactivos que
             requieras para mostrar alertas o pasos

             estos ejemplos se pueden eliminar para cambiarlos 
             por los que sean necesarios, solo son ejemplos
            */
            Message: 'Hola desde modelo de Vue',
            Array: [],
            Object: {},
            Int: 0,
            boolean: false,
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

        //Aquí llamarás a los métodos que quieres que se monten con la página cuando está iniciando
        console.log('Hola Mundo desde Vue (consola)');
    }
});
app.mount('#app');