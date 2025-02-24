const { createApp } = Vue;

const app = createApp({
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
            notes: '',
            municipality: {
                Id_Municipality: 0,
                Name: '',
                Zip_Code: 0,
            },
            consultory: {
                Id_Municipality: 0,
                Name: '',
                Zip_Code: 0,
            },
            municipalities: [],
            consultories: [],
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

        getMunicipalities() {
            axios.get(createUser)
                .then(response => {

                    this.municipalities = response.data;

                    console.log(response.data, 'respuesta del método');
                })
                .catch(error => {
                    console.log(error, 'Mensaje de error')
                });
        },

        //Método que se ejecuta cuando seleccionamos otro municipio
        onMunicipalityChange(municipality) {

            //Igualar el modelo de vue a los valores del objeto seleccionado
            this.municipality = {
                Id_Municipality: municipality.Id_Municipality,
                Name: municipality.Name,
                Zip_Code: municipality.Zip_Code,
            };

            this.getConsultories(municipality.Id_Municipality);
        },

        getConsultories(Id_Municipality) {
            axios.get(getConsultories)
                .then(response => {

                    this.consultories = response.data;

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