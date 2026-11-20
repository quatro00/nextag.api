using Nextag.Api.Services.Notificaciones.Interface;
using System.Text.RegularExpressions;

namespace Nextag.Api.Services.Notificaciones.Implementation
{
    public class TelefonoWhatsAppService
        : ITelefonoWhatsAppService
    {
        public string Normalizar(
            string telefono,
            string? codigoPais = null)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                throw new ArgumentException(
                    "El teléfono es obligatorio.");
            }

            var original = telefono.Trim();

            var internacional =
                original.StartsWith("+");

            // ======================================
            // NORMALIZAR SEPARADORES
            // ======================================

            var numero = Regex.Replace(
                original,
                @"[\s\-\(\)\.]",
                "");

            if (numero.StartsWith("+"))
            {
                numero = numero[1..];
            }

            if (!Regex.IsMatch(numero, @"^\d+$"))
            {
                throw new ArgumentException(
                    "El teléfono contiene caracteres no válidos.");
            }

            // ======================================
            // CODIGO DE PAIS
            // ======================================

            var prefijo = string.IsNullOrWhiteSpace(codigoPais)
                ? "52"
                : Regex.Replace(codigoPais, @"[\s+]", "");

            if (!Regex.IsMatch(
                prefijo,
                @"^[1-9]\d{0,2}$"))
            {
                throw new ArgumentException(
                    "El código de país no es válido.");
            }

            // ======================================
            // FORMATO INTERNACIONAL EXPLICITO
            // ======================================

            if (internacional)
            {
                return ValidarInternacional(numero);
            }

            // ======================================
            // MEXICO
            // ======================================

            if (prefijo == "52")
            {
                // Número nacional de 10 dígitos.

                if (numero.Length == 10)
                {
                    return ValidarInternacional(
                        "52" + numero);
                }

                // Número con código de país.

                if (numero.Length == 12 &&
                    numero.StartsWith("52"))
                {
                    return ValidarInternacional(numero);
                }

                throw new ArgumentException(
                    "El teléfono mexicano debe tener " +
                    "10 dígitos o incluir el prefijo 52.");
            }

            // ======================================
            // OTROS PAISES
            // ======================================

            // Cuando el país no es México,
            // un número sin '+' puede ser ambiguo.
            //
            // Exigimos formato internacional explícito
            // para evitar enviar al país equivocado.

            throw new ArgumentException(
                "Para teléfonos internacionales " +
                "utiliza el formato + seguido del " +
                "código de país y número completo.");
        }

        private static string ValidarInternacional(
            string numero)
        {
            // Validación estructural E.164.
            // No verifica que el número exista.

            if (!Regex.IsMatch(
                numero,
                @"^[1-9]\d{7,14}$"))
            {
                throw new ArgumentException(
                    "El teléfono internacional " +
                    "debe contener entre 8 y 15 dígitos.");
            }

            return numero;
        }
    }
}
