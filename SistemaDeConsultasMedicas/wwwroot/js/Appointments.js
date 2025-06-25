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

            Authorization: {
                Success: false,
                User: null,
            },

            //Modelo para credenciales del usuario
            credentials: {
                Email: "",
                Password: "",
            },

            //Objeto donde se pondrá la configuración para buscar la API y parámetros que recibe
            config: {
                IdApi: null, //Id de la API (Base de datos)
                BodyParams: null, //Objeto (Generalmente para métodos tipo "Post")
                Param: null, //Objeto exclusivo para ÁPIs tipo "Get"
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

        getStatuses() {
            //Id para obtener los estatus de la lista xdd
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

        //Obtener los consultorios
        getDoctors() {
            this.config = {
                IdApi: 3,
                BodyParams: null,
                Param: `${this.selectedConsultory}`, //Se transforma el valor en string
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

        //Obtener los consultorios
        getAppointments() {
            this.config = {
                IdApi: 4,
                BodyParams: null,
                Param: `${this.user.id_User}`, //Se transforma el valor en string
            };

            axios.post(window.callApiAsync, this.config)
                .then(response => {

                    this.appointments = response.data;
                    console.log('citas', response.data);

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

        submitAppointment() {
            this.isLoading = true;

            this.config = {
                IdApi: 5,
                BodyParams: {
                    fk_Doctor: this.selectedDoctor,
                    fk_Patient: this.user.id_User,
                    notes: this.notes,
                },
                Param: null, //Se transforma el valor en string
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

                        if (this.user.fk_Role === 1) window.location.reload();
                        else window.location.reload();

                    });
                })
                .catch(error => {
                    console.error("Error en la petición:", error);
                });
        },

        deleteAppointment(appointment) {
            this.config = {
                IdApi: 1014, //Cambiar por 15 cuando se solucione el problema de los Ids
                BodyParams: {
                    id_Appointment: appointment.id,
                    fk_Doctor: appointment.fk_Doctor,
                    fk_Patient: appointment.fk_Patient,
                },
                Param: null, //Se transforma el valor en string
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
        },
    },
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

        //Aquí llamarás a los métodos que quieres que se monten con la página cuando está iniciando
        this.getMunicipalities();
        this.getStatuses();
    }
});
app.mount('#app');