using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Filters;

namespace Nextag.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiereEmpresaAttribute : TypeFilterAttribute
    {
        public RequiereEmpresaAttribute() : base(typeof(ValidarEmpresaFilter))
        {
        }
    }
}
