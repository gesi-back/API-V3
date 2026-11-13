using GESI.CORE.API.BO.V3;
using GESI.ERP.Core.V3.BLL;

namespace GESI.CORE.API.BLL.V3
{
    public interface IDivisasServicio
    {
        cResponseDivisas GetList(int pageNumber, int pageSize, int divisaID, DateTime? fechaDesde, DateTime? fechaHasta, bool ultimaCotizacion);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.DivisasMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class DivisasServicio(
        IDivisasBLL divisasBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IDivisasServicio
    {
        public cResponseDivisas GetList(int pageNumber, int pageSize, int divisaID,
                                        DateTime? fechaDesde, DateTime? fechaHasta, bool ultimaCotizacion)
        {
            if (fechaDesde is not null && fechaHasta is not null && fechaDesde > fechaHasta)
                throw new ArgumentException("fechaDesde no puede ser mayor que fechaHasta");   // → 400

            return Paginar(new cResponseDivisas(),
                           () => divisasBLL.GetList(divisaID, fechaDesde, fechaHasta, ultimaCotizacion),
                           pageNumber, pageSize,
                           endpoint: Endpoint.Maestros.Divisas("GetList"),
                           nombreLog: "Divisas GetList",
                           mensajeNoEncontrado: "No se encontraron divisas",
                           asignarItems: (r, items) => r.divisas = items);
        }
    }
}
