const app = Vue.createApp({
    data() {
        return {
            isLogin: true,
            
            //Modelo para credenciales del usuario
            credentials: {
                Email: '',
                Password: '',
            },

            //Modelo para manejar autorización y almacenar datos del usuario
            Authorization: {
                Success: false,
                User: null,
            },

            //Modelo para nuevos usuarios
            newUser: {
                id_User: 0,
                name: '',
                email: '',
                password: '',
                phone: '',
                fk_Sex: '',
                fk_Role: 2,
            },

            confirmPassword: '',

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
            const validation = this.validateCredentials(this.credentials)
            if (!validation.isValid) {

                this.credentials.Email = '';
                this.credentials.Password = '';

                Swal.fire({
                    title: "Advertencia",
                    text: "Los campos contienen indicios de inyección",
                    icon: "error",
                    confirmButtonText: 'Entendido',
                    confirmButtonColor: '#3085d6'
                });
                return;
            }

            this.config = {
                IdApi: 9,
                BodyParams: this.credentials,
                Param: null,
            };

            Swal.fire({
                title: "Iniciando sesión...",
                text: "Por favor, espera",
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Usuario', response.data);

                    if (response.data.id_User != 0) {

                        Swal.fire({
                            title: "Éxito",
                            text: "Inicio de sesión exitoso",
                            icon: "success",
                            timer: 1500,
                            allowOutsideClick: false,
                            showConfirmButton: false
                        }).then(() => {
                            this.goToIndex(response.data.id_User);
                        })

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
                            email: '',
                            password: '',
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

        register() {
            const validation = this.validateCredentials({
                Email: this.newUser.email,
                Password: this.newUser.password,
            });

            if (this.newUser.password === this.confirmPassword) {
                Swal.fire({
                    title: "Error",
                    text: "Las contraseñas son diferentes",
                    icon: "error",
                    timer: 1500,
                    showConfirmButton: false
                });
                return;
            }

            if (!this.validatePassword()) {
                Swal.fire({
                    title: "Error",
                    text: "La contraseña no cumple con los requisitos de seguridad",
                    icon: "error",
                    confirmButtonText: 'Entendido',
                    confirmButtonColor: '#3085d6'
                });
                return;
            }

            if (!validation.isValid) {
                Swal.fire({
                    title: "Advertencia",
                    text: validation.error,
                    icon: "error",
                    confirmButtonText: 'Entendido',
                    confirmButtonColor: '#3085d6'
                });
                return;
            }

            this.config = {
                IdApi: 11, // Cambiar según el Id de tu API para registrar
                BodyParams: this.newUser,
                Param: null,
            };

            console.log('Registrando usuario', this.newUser);

            Swal.fire({
                title: "Registrando usuario...",
                text: "Por favor, espera",
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            axios.post(window.callApiAsync, this.config)
                .then(response => {
                    console.log('Respuesta de registro', response.data);

                    if (response.data.success) {
                        Swal.fire({
                            title: "Éxito",
                            text: response.data.message || "Usuario registrado correctamente",
                            icon: "success",
                            timer: 2000,
                            allowOutsideClick: false,
                            showConfirmButton: false
                        }).then(() => {
                            // Puedes redirigir o pasar automáticamente a login
                            this.isLogin = true;
                            // Limpiar campos del formulario
                            this.newUser = {
                                id_User: 0,
                                name: '',
                                email: '',
                                password: '',
                                phone: '',
                                fk_Sex: '',
                                fk_Role: 2,
                            };
                        });
                    } else {
                        Swal.fire({
                            title: "Error",
                            text: response.data.message || "No se pudo registrar el usuario",
                            icon: "error",
                            timer: 1500,
                            showConfirmButton: false
                        });
                    }
                })
                .catch(error => {
                    console.error("Error en la petición de registro:", error);
                    Swal.fire({
                        title: "Error",
                        text: "Hubo un error al registrar el usuario",
                        icon: "error",
                        timer: 1500,
                        showConfirmButton: false
                    });
                });
        },

        goToIndex(idUser) {
            const root = window.index || null;
            if (root) window.location.href = `${root}?id=${idUser}`;
        },

        validatePassword() {
            let success = true;
            const minLength = 8;
            const hasUpper = /[A-Z]/.test(this.newUser.password);
            const hasLower = /[a-z]/.test(this.newUser.password);
            const hasNumber = /\d/.test(this.newUser.password);
            const hasSymbol = /[^A-Za-z0-9]/.test(this.newUser.password);

            let errors = [];

            if (this.newUser.password.length < minLength) errors.push(`Debe tener al menos ${minLength} caracteres`);
            if (!hasUpper) errors.push("Debe incluir al menos una letra mayúscula");
            if (!hasLower) errors.push("Debe incluir al menos una letra minúscula");
            if (!hasNumber) errors.push("Debe incluir al menos un número");
            if (!hasSymbol) errors.push("Debe incluir al menos un símbolo (@, #, $, %, &)");

            this.passwordErrors = errors;

            // Si hay errores, mostrar alerta
            if (errors.length > 0) {
                success = false;

                Swal.fire({
                    icon: 'warning',
                    title: 'Contraseña insegura',
                    html: errors.map(err => `<p>${err}</p>`).join(''),
                    confirmButtonText: 'Entendido',
                    confirmButtonColor: '#3085d6'
                });
            }
            return success;
        },

        validateCredentials(credentials) {
            // Verificar que sea un objeto
            if (typeof credentials !== 'object' || credentials === null) {
                return {
                    isValid: false,
                    error: 'El parámetro debe ser un objeto con Email y Password.'
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

            for (let key in credentials) {
                const value = credentials[key];

                // Validar que sea string
                if (typeof value !== 'string') {
                    return {
                        isValid: false,
                        error: `El campo ${key} debe ser una cadena de texto.`
                    };
                }

                // Validar patrones de inyección
                for (let pattern of injectionPatterns) {
                    if (pattern.test(value)) {
                        return {
                            isValid: false,
                            error: `El campo ${key} contiene patrones peligrosos de inyección.`
                        };
                    }
                }
            }

            return {
                isValid: true,
                error: null
            };
        },
    },
});
app.mount('#app');