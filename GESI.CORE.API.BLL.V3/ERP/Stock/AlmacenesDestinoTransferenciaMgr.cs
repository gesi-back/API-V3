using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Stock;
using GESI.ERP.Stock.V3.BLL;

namespace GESI.CORE.API.BLL.V3.ERP.Stock
{
    public interface IAlmacenesDestinoTransferenciaServicio
    {
        cResponseAlmacenesDestintoTransferencia GetList(int pageNumber, int pageSize, short almacenID);
    }

    /// <summary>
    /// Migrado de la V2 (cAlmacenesDestinoTransferenciaMgr estático con _MiAPISessionMgr estático).
    /// </summary>
    internal sealed class AlmacenesDestinoTransferenciaServicio(
        IAlmacenDestinoTransferenciaBLL almacenDestinoTransferenciaBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IAlmacenesDestinoTransferenciaServicio
    {
        /// <summary>
        /// Devuelve los almacenes a los que se puede transferir desde el almacén indicado
        /// </summary>
        public cResponseAlmacenesDestintoTransferencia GetList(int pageNumber, int pageSize, short almacenID)
        {
            // NUEVO respecto de la V2: sin almacenID devuelve 400 (la V2 devolvía 200 con success = true y el error adentro)
            if (almacenID == 0)
                throw new cCodigoNoDetalladoException("No se detalló el almacenID");   // → 400

            return Paginar(new cResponseAlmacenesDestintoTransferencia(),
                           () => almacenDestinoTransferenciaBLL.GetList(almacenID),
                           pageNumber, pageSize,
                           endpoint: Endpoint.Maestros.AlmacenesDestinoTransferencia("GetList"),
                           nombreLog: "AlmacenesDestinoTransferencia GetList",
                           mensajeNoEncontrado: "No se encontraron almacenes de destino",
                           asignarItems: (r, items) => r.almacenes = items);
        }
    }
}
