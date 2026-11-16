using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IReferenciasContablesServicio
    {
        cResponseReferenciasContables GetList(int pageNumber, int pageSize);
        cResponseReferenciaContable GetItem(int referenciaContableID);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.ReferenciasContablesMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class ReferenciasContablesServicio(
        IReferenciasContablesBLL referenciasContablesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IReferenciasContablesServicio
    {
        public cResponseReferenciasContables GetList(int pageNumber, int pageSize)
            => Paginar(new cResponseReferenciasContables(),
                       () => referenciasContablesBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.ReferenciasContables("GetList"),
                       nombreLog: "Referencia Contables GetList",
                       mensajeNoEncontrado: "No se encontraron referencias contables",
                       asignarItems: (r, items) => r.referenciasContables = items);

        public cResponseReferenciaContable GetItem(int referenciaContableID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (referenciaContableID == 0)
                throw new cCodigoNoDetalladoException("Se debe especificar el parametro referenciaContableID");   // → 400

            // El ID llega como int (igual que la V2) y recién acá se pasa a texto: así el Core nunca recibe
            // texto libre del cliente en el filtro de IDs.
            var referencia = referenciasContablesBLL.GetList(referenciaContableID.ToString())?.FirstOrDefault()
                             ?? throw new cDatosNoEncontradosException("No se encontró la referencia contable " + referenciaContableID);   // → 404

            LoguearInfo("Success Referencia Contables GetItem", Endpoint.Maestros.ReferenciasContables("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseReferenciaContable
            {
                referenciaContable = referencia,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }
    }
}
