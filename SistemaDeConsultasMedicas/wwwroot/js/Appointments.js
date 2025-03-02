const { createApp } = Vue;

const app = createApp({
    data() {
        return {
            municipalities: [
                { id: 1, name: 'Municipio 1' },
                { id: 2, name: 'Municipio 2' },
                { id: 3, name: 'Municipio 3' }
            ],
            consultories: [
                { id: 1, name: 'Consultorio 1', municipalityId: 1 },
                { id: 2, name: 'Consultorio 2', municipalityId: 1 },
                { id: 3, name: 'Consultorio 3', municipalityId: 2 },
                { id: 4, name: 'Consultorio 4', municipalityId: 3 }
            ],
            doctors: [
                { id: 1, name: 'Doctor 1', consultoryId: 1 },
                { id: 2, name: 'Doctor 2', consultoryId: 1 },
                { id: 3, name: 'Doctor 3', consultoryId: 2 },
                { id: 4, name: 'Doctor 4', consultoryId: 3 },
                { id: 5, name: 'Doctor 5', consultoryId: 4 }
            ],
            selectedMunicipality: null,
            selectedConsultory: null,
            selectedDoctor: null,
            notes: '',
            isLoading: false,
            showSuccessMessage: false,
            showErrorMessage: false,
            errorMessage: ''
        };
    },
    computed: {
        filteredConsultories() {
            return this.consultories.filter(consultory => consultory.municipalityId === this.selectedMunicipality);
        },
        filteredDoctors() {
            return this.doctors.filter(doctor => doctor.consultoryId === this.selectedConsultory);
        }
    },
    methods: {
        onMunicipalityChange() {
            this.selectedConsultory = null;
            this.selectedDoctor = null;
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
        }
    },
    mounted() {

        //Aquí llamarás a los métodos que quieres que se monten con la página cuando está iniciando
        console.log('Hola Mundo desde Vue (consola)');
    }
});
app.mount('#app');