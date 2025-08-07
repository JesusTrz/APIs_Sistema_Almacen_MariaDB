using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class UsuariosDto
    {

        public int ID_Usuario { get; set; }
        public string Nombre_Usuario { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
        ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula y un número.")]
        public string Contrasenia { get; set; }
        public Nullable<int> ID_Roles { get; set; }
        public string Nombre_Rol { get; set; }
        public Nullable<int> ID_Sede { get; set; }
    }
    public class UsuariosDatos
    {
        public string Nombre_Usuario { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
        ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula y un número.")]
        public string Contrasenia { get; set; }
        public Nullable<int> ID_Roles { get; set; }
        public Nullable<int> ID_Sede { get; set; }
    }

    public class CambioContrasenia
    {
        public string ViejaContrasenia { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
        ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula y un número.")]
        public string NuevaContrasenia { get; set; }
    }

    public class LoginUser
    {
        public string Nombre_Usuario { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
        ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula y un número.")]
        public string Contrasenia { get; set; }
    }
}