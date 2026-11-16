using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Ventas;
using GESI.CORE.API.BO.V3.ERP.Ventas;
using GESI.ERP.Ventas.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace API_con_Core_V3.Controllers
{
    /// <summary>
    /// Controlador de comprobantes pendientes de clientes (facturas, recibos, notas de credito y debito)
    /// </summary>
    [ApiController]
    [Route("api/Ventas/Comprobantes/[controller]")]
    [Produces("application/json")]
    public class GetComprobantesPendientesController(IMovimientosDeClientesServicio MovimientosDeClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene un listado de comprobantes pendientes
        /// </summary>
        /// <param name="fechaHasta">Fecha hasta (por defecto, hoy)</param>
        /// <param name="clienteID">Filtra por cliente</param>
        /// <param name="sucursalID">Filtra por sucursal</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="incluirFojaID">Devuelve la foja de reparto de cada comprobante (S/N)</param>
        /// <param name="fechaDesde">Fecha desde (por defecto, 01/01/2000)</param>
        [HttpGet]
        public ActionResult<cResponseMovimientoDeClientes> GetComprobantesPendientesClientes(string fechaHasta = "",
                                                                                             int clienteID = 0,
                                                                                             int sucursalID = 0,
                                                                                             int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                                             int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                                             string incluirFojaID = "N",
                                                                                             string fechaDesde = "")
            => Responder(() =>
            {
                // Igual que la V2: sin fechaHasta usa hoy y sin fechaDesde usa 01/01/2000 (fecha inválida → 400)
                DateTime oFechaHasta = fechaHasta?.Length > 0 ? DateTime.Parse(fechaHasta) : DateTime.Now;
                DateTime oFechaDesde = fechaDesde?.Length > 0 ? DateTime.Parse(fechaDesde) : new DateTime(2000, 1, 1);

                string strSubTipos = SubTiposComprobantes.sFactura + "," + SubTiposComprobantes.sRecibo + "," + SubTiposComprobantes.sNCR + "," + SubTiposComprobantes.sNDB;

                dParametrosConsultaDeComprobantesDatosDeLaOperacion oParametro = new dParametrosConsultaDeComprobantesDatosDeLaOperacion();
                oParametro.estadoPendiente = EstadosComprobantes.ePendiente;
                oParametro.fechaHasta = oFechaHasta;
                oParametro.fechaDesde = oFechaDesde;

                if (clienteID > 0)
                    oParametro.clienteID = clienteID;

                if (sucursalID > 0)
                    oParametro.sucursalID = sucursalID;

                return MovimientosDeClientesMgr.GetList(oParametro,
                                                        null,
                                                        strSubTipos,
                                                        pageNumber: pageNumber,
                                                        pageSize: pageSize,
                                                        DevolverFojaID: incluirFojaID.Equals("S"));
            });
    }
}
