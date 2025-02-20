using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json;
#pragma warning disable CS8618

namespace WebServices.Services
{
    public class Services
    {
    }

    public class APIMunicipality
    {
        public List<Municipalities> data { get; set; }
    }
    public class APIspecialties
    {
        public List<Specialties> data { get; set; }
    }

    public class Municipalities
    {
        [JsonPropertyName("Geography ID")]
        public double GeographyID { get; set; }

        [JsonPropertyName("Geography")]
        public string Geography { get; set; }

        [JsonPropertyName("Municipality ID")]
        public int MunicipalityID { get; set; }

        [JsonPropertyName("Municipality")]
        public string Municipality { get; set; }

        [JsonPropertyName("Codigo Postal")]
        public int CodigoPostal { get; set; }

        [JsonPropertyName("Establishment Type ID")]
        public int EstablishmentTypeID { get; set; }

        [JsonPropertyName("Establishment Type")]
        public string EstablishmentType { get; set; }

        [JsonPropertyName("Latitud")]
        public string Latitud { get; set; }

        [JsonPropertyName("Longitud")]
        public string Longitud { get; set; }
    }

    public class Specialties
    {
        [JsonPropertyName("Resources Subcategories")]
        public string Name { get; set; }
    }
}
