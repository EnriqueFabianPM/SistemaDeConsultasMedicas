const { createApp } = Vue;

const app = createApp({
    data() {
        return {
            municipalities: [],
            consultories: [],
            doctors: [],
            selectedMunicipality: null,
            selectedConsultory: null,
            selectedDoctor: null,
            notes: '',
            isLoading: false,
            showSuccessMessage: false,
            showErrorMessage: false,
            errorMessage: '',

            //Objeto donde se pondrá la configuración para buscar la API y parámetros que recibe
            config: {
                IdApi: null, //Id de la API (Base de datos)
                BodyParams: null, //Objeto (Generalmente para métodos tipo "Post")
                Param: null, //Objeto exclusivo para ÁPIs tipo "Get"
            }
        };
    },
    methods: {
        onMunicipalityChange() {
            this.selectedConsultory = null;
            this.selectedDoctor = null;

            this.getConsultories();
        },

        onConsultoryChange() {
            this.selectedDoctor = null;
        },

        async submitAppointment() {
            if (!this.selectedMunicipality || !this.selectedConsultory || !this.selectedDoctor) {
                this.showErrorMessage = true;
                this.errorMessage = "Por favor, complete todos los campos obligatorios.";
                setTimeout(() => {
                    this.showErrorMessage = false;
                }, 3000);
                return;
            }

            this.isLoading = true;

            const appointment = {
                municipalityId: this.selectedMunicipality,
                consultoryId: this.selectedConsultory,
                doctorId: this.selectedDoctor,
                notes: this.notes
            };

            try {
                // Simulación de una llamada a la API
                const response = await fetch('https://api.example.com/appointments', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(appointment)
                });

                if (!response.ok) throw new Error('Error al crear la cita');

                console.log('Cita creada con éxito:', appointment);

                // Resetear el formulario
                this.selectedMunicipality = null;
                this.selectedConsultory = null;
                this.selectedDoctor = null;
                this.notes = '';

                // Mostrar mensaje de éxito
                this.showSuccessMessage = true;
                setTimeout(() => {
                    this.showSuccessMessage = false;
                }, 3000);
            } catch (error) {
                console.error('Error:', error);
                this.showErrorMessage = true;
                this.errorMessage = 'Hubo un error al crear la cita. Por favor, inténtelo de nuevo.';
                setTimeout(() => {
                    this.showErrorMessage = false;
                }, 3000);
            } finally {
                this.isLoading = false;
            }
        },

        //Obtener los municipios
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
        getConsultories() {

            this.config = {
                IdApi: 2, //Consultorios
                BodyParams: null, //Al ser una api tipo Get se manda null
                Param: `${this.selectedMunicipality}`, //Se transforma el valor en string
            };

            axios.post(window.callApiAsync, this.config)
                .then(response => {

                    this.consultories = response.data;
                    console.log('Consultorios', response.data);
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });

        },


    //    postApi() {
    //        console.log('configuración',this.config);

    //        axios.post(window.callApiAsync, this.config)  // Enviar directamente el objeto "api"
    //            .then(response => {
    //                console.log('Respuesta del método genérico',response.data);
    //            })
    //            .catch(error => {
    //                console.error("Error en la petición:", error);
    //            });
    //    }
    },
    mounted() {

        //Aquí llamarás a los métodos que quieres que se monten con la página cuando está iniciando
        this.getMunicipalities();
    }
});
app.mount('#app');