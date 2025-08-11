const { createApp } = Vue;

const app = createApp({
    data() {
        return {
            map: null,
            municipalities: [],
            consultories: [],
            doctors: [],
            appointments: [],
            selectedMunicipality: null,
            selectedConsultory: null,
            selectedDoctor: null,
            notes: '',
            isLoading: false,
            showSuccessMessage: false,
            showErrorMessage: false,
            errorMessage: '',
            statuses: [],
            currentStatuses: [],

            Authorization: {
                Success: false,
                User: null,
            },

            // Modelo para credenciales del usuario
            credentials: {
                Email: "",
                Password: "",
            },

            // Objeto donde se pondrá la configuración para buscar la API y parámetros que recibe
            config: {
                IdApi: null, // Id de la API (Base de datos)
                BodyParams: null, // Objeto (Generalmente para métodos tipo "Post")
                Param: null, // Objeto exclusivo para ÁPIs tipo "Get"
            },

            user: {},
        };
    },
    methods: {
        generateMap() {
            this.map = new google.maps.Map(document.getElementById("map"), {
                center: { lat: 19.4326, lng: -99.1332 },
                zoom: 12,
            });
        },

        updateMapMarkers() {
            const consultory = this.consultories.find(o => o.id === this.selectedConsultory);
            const map = this.map;

            if (consultory) {
                const position = {
                    lat: parseFloat(consultory.latitude),
                    // Nota: aquí usabas 'length' — si tu API devuelve 'longitude' o 'lng' probablemente debas cambiar este campo.
                    lng: parseFloat(consultory.length)
                };

                // Si ya hay un marcador previo, lo movemos
                if (this.marker) {
                    this.marker.setPosition(position);
                } else {
                    // Si no existe aún, lo creamos
                    this.marker = new google.maps.Marker({
                        position,
                        map,
                        title: consultory.name
                    });
                }

                map.setCenter(position);
            }
        },

        onMunicipalityChange() {
            this.selectedConsultory = null;
            this.selectedDoctor = null;

            this.getConsultories();
            if (!this.map) this.generateMap();
        },

        onConsultoryChange() {
            this.selectedDoctor = null;
            this.getDoctors();

            this.updateMapMarkers();
        },

        // Obtener los municipios
        getMunicipalities() {
            // Id para obtener los municipios
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

        getStatuses() {
            // Id para obtener los estatus
            this.config.IdApi = 14;

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Statuses', response.data);
                    this.statuses = response.data;
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        // Obtener los consultorios
        getConsultories() {
            this.config = {
                IdApi: 2, // Consultorios
                BodyParams: null, // Al ser una api tipo Get se manda null
                Param: `${this.selectedMunicipality}`, // Se transforma el valor en string
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

        // Obtener los doctores
        getDoctors() {
            this.config = {
                IdApi: 3,
                BodyParams: null,
                Param: `${this.selectedConsultory}`, // Se transforma el valor en string
            };

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    this.doctors = response.data;
                    console.log('Doctores', response.data);
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        // Obtener las citas
        getAppointments() {
            this.config = {
                IdApi: 4,
                BodyParams: null,
                Param: `${this.user.id_User}`, // Se transforma el valor en string
            };

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    this.appointments = response.data;
                    console.log('citas', response.data);

                    if (this.appointments) {
                        this.appointments.forEach(appointment => {
                            this.currentStatuses[appointment.id] = appointment.fk_Status;
                        });
                    }
                    console.log('status del servidor', this.currentStatuses);

                    this.$nextTick(() => {
                        $(this.$refs.appointmentsTable).DataTable({
                            paging: true,
                            searching: true,
                            ordering: true,
                            responsive: true,
                            scrollY: this.user.fk_Role === 1 ? '225px' : '600px',
                            scrollCollapse: true,
                            language: {
                                processing: "Procesando...",
                                search: "Buscar:",
                                lengthMenu: "Mostrar _MENU_ registros",
                                info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
                                infoEmpty: "No hay registros disponibles",
                                infoFiltered: "(filtrado de _MAX_ registros en total)",
                                loadingRecords: "Cargando...",
                                zeroRecords: "No se encontraron registros",
                                emptyTable: "No hay datos disponibles en la tabla",
                                paginate: {
                                    first: "Primero",
                                    previous: "Anterior",
                                    next: "Siguiente",
                                    last: "Último"
                                },
                                aria: {
                                    sortAscending: ": activar para ordenar ascendente",
                                    sortDescending: ": activar para ordenar descendente"
                                }
                            }
                        });
                    });
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        validateCredentials() {
            // Verificar que notes sea una cadena y no esté vacía
            if (typeof this.notes !== 'string' || this.notes === '') {
                return {
                    isValid: false,
                    error: `El campo notas debe ser una cadena de texto.`
                };
            }

            // Lista de patrones sospechosos de inyección
            const injectionPatterns = [
                /(\b)(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|CREATE|TRUNCATE)(\b)/i, // SQL keywords
                /(;|--|\bOR\b|\bAND\b).*=.*/i, // SQL injection lógica
                /<script.*?>.*?<\/script>/i, // Script tags
                /javascript:/i, // URI scheme JS
                /\b(alert|prompt|confirm|eval|Function)\s*\(/i // JS dangerous functions
            ];

            // Validar patrones de inyección en this.notes
            for (let pattern of injectionPatterns) {
                if (pattern.test(this.notes)) {
                    return {
                        isValid: false,
                        error: 'El campo de notas adicionales contiene patrones peligrosos de inyección.'
                    };
                }
            }

            return {
                isValid: true,
                error: null
            };
        },

        submitAppointment() {
            const validation = this.validateCredentials();
            if (!validation.isValid) {
                Swal.fire({
                    title: "Advertencia",
                    text: validation.error,
                    icon: "error",
                    timer: 1500,
                    showConfirmButton: false
                });
                this.isLoading = false;
                this.notes = '';
                return;
            
            }
            this.isLoading = true;

            this.config = {
                IdApi: 5,
                BodyParams: {
                    fk_Doctor: this.selectedDoctor,
                    fk_Patient: this.user.id_User,
                    notes: this.notes,
                },
                Param: null,
            };

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    Swal.fire({
                        title: "¡Listo!",
                        text: `${response.data.message}`,
                        icon: "success",
                        timer: 1500,
                        showConfirmButton: false,
                        allowClickOutside: false,
                    }).then(() => {
                        this.isLoading = false;
                        window.location.reload();
                    });
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                    this.isLoading = false;
                });
        },

        updateAppointment(appointment) {
            if (appointment) {
                appointment.fk_Status = this.currentStatuses[appointment.id];
                this.config = {
                    IdApi: 13,
                    BodyParams: {
                        id_Appointment: appointment.id,
                        fk_Doctor: appointment.fk_Doctor,
                        fk_Patient: appointment.fk_Patient,
                        fk_Status: appointment.fk_Status,
                    },
                    Param: null,
                };

                console.log("Appointment a actualizar para debug", this.config.BodyParams);

                axios.post(window.callApiAsync, this.config)
                    .then(response => {
                        Swal.fire({
                            title: "¡Listo!",
                            text: `${response.data.message}`,
                            icon: "success",
                            timer: 1500,
                            showConfirmButton: false,
                            allowClickOutside: false,
                        }).then(() => {
                            window.location.reload();
                        });
                    })
                    .catch(error => console.error("Error en la petición:", error));
            }
        },

        deleteAppointment(appointment) {
            this.config = {
                IdApi: 15,
                BodyParams: {
                    id_Appointment: appointment.id,
                    fk_Doctor: appointment.fk_Doctor,
                    fk_Patient: appointment.fk_Patient,
                },
                Param: null,
            };

            console.log("Appointment a borrar", this.config.BodyParams);

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    Swal.fire({
                        title: "¡Listo!",
                        text: `${response.data.message}`,
                        icon: "success",
                        timer: 1500,
                        showConfirmButton: false,
                        allowClickOutside: false,
                    }).then(() => {
                        window.location.reload();
                    });
                })
                .catch(error => console.error("Error en la petición:", error));
        }
    }, // end methods

    mounted() {
        console.log(window.user); // Ahora es un objeto JSON usable
        if (window.user?.id_User) {
            this.Authorization = {
                Success: true,
                User: null,
            };

            this.user = window.user;
            this.$nextTick(() => {
                if (this.user.fk_Role !== 2) this.getAppointments();
            });
        }

        // Aquí llamarás a los métodos que quieres que se monten con la página cuando está iniciando
        this.getMunicipalities();
        this.getStatuses();
    }
});

app.mount('#app');
