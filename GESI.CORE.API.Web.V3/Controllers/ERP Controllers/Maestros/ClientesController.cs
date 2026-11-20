using GESI.CORE.API.BLL.V3;
using GESI.CORE.API.BLL.V3.ERP.Maestros;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BO;
using Microsoft.AspNetCore.Mvc;

namespace GESI.CORE.API.Web.V3.Controllers
{
    /// <summary>
    /// Controlador de clientes
    /// Documentación: https://docs.google.com/document/d/1a_tZH2rsi8OHt-MgiClgPHHjHaBnimFLW9g2cImfOJk/edit
    /// </summary>
    [ApiController]
    [Route("api/Maestros/[controller]")]
    [Produces("application/json")]
    public class ClientesController(IClientesServicio ClientesMgr) : ApiControllerBase
    {
        /// <summary>
        /// Obtiene la lista de clientes
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="fechamodificaciones">Devuelve los clientes a partir de una fecha de modificacion</param>
        /// <param name="sucursales">Define si se devuelven las sucursales (S/N)</param>
        /// <param name="preciosespeciales">Define si se devuelven los precios especiales (S/N)</param>
        /// <param name="contactos">Define si se devuelven los contactos (S/N)</param>
        /// <param name="unidadesdecliente">Define si se devuelven las unidades de cada cliente (S/N)</param>
        /// <param name="tiposDeCliente">Permite filtrar por tipo de cliente</param>
        /// <param name="canalesDeVenta">Permite filtrar por canales de venta</param>
        /// <param name="formasDePago">Permite filtrar por formas de pago</param>
        /// <param name="condicionesIVA">Permite filtrar por condiciones de IVA</param>
        /// <param name="ordenarPor">Marca el campo por el que se ordena la lista</param>
        /// <param name="direccion">Marca el tipo de ordenamiento (asc, desc)</param>
        /// <param name="definicionesSucursal">Define si se devuelven las definiciones por sucursal (S/N)</param>
        [HttpGet("GetList")]
        public ActionResult<cResponseClientes> GetList(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                       int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                       string fechamodificaciones = "",
                                                       string sucursales = "N",
                                                       string preciosespeciales = "N",
                                                       string contactos = "N",
                                                       string unidadesdecliente = "N",
                                                       string? tiposDeCliente = null,
                                                       string? canalesDeVenta = null,
                                                       string? formasDePago = null,
                                                       string? condicionesIVA = null,
                                                       string? ordenarPor = null,
                                                       string? direccion = null,
                                                       string definicionesSucursal = "N")
            => Responder(() => ClientesMgr.GetList(pageNumber,
                                                   pageSize,
                                                   sucursales,
                                                   preciosespeciales,
                                                   fechamodificaciones,
                                                   contactos,
                                                   unidadesdecliente,
                                                   tiposDeCliente,
                                                   canalesDeVenta,
                                                   formasDePago,
                                                   condicionesIVA,
                                                   ordenarPor,
                                                   direccion,
                                                   definicionesSucursal));

        /// <summary>
        /// Obtiene un cliente determinado
        /// </summary>
        /// <param name="clienteID">Codigo de cliente</param>
        /// <param name="sucursales">Define si se devuelven las sucursales del cliente (S/N)</param>
        /// <param name="preciosespeciales">Define si se devuelven los precios especiales del cliente (S/N)</param>
        /// <param name="contactos">Define si se devuelven los contactos del cliente (S/N)</param>
        /// <param name="unidadesdecliente">Define si se devuelven las unidades del cliente (S/N)</param>
        [HttpGet("GetItem")]
        public ActionResult<cResponseCliente> GetItem(int clienteID = 0,
                                                      string sucursales = "N",
                                                      string preciosespeciales = "N",
                                                      string contactos = "N",
                                                      string unidadesdecliente = "N")
            => Responder(() => ClientesMgr.GetItem(clienteID,
                                                   sucursales,
                                                   preciosespeciales,
                                                   contactos,
                                                   unidadesdecliente));

        /// <summary>
        /// Obtiene la lista de clientes por un criterio de busqueda (razon social, contacto, email, telefono, etc.)
        /// </summary>
        /// <param name="expresion">Expresion a buscar en la lista de clientes</param>
        /// <param name="buscardatosdecontacto">Define si el criterio busca en los datos de contacto (S/N)</param>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="sucursales">Define si se devuelven las sucursales de los clientes (S/N)</param>
        /// <param name="preciosespeciales">Define si se devuelven los precios especiales (S/N)</param>
        /// <param name="fechamodificaciones">Se recibe por compatibilidad con la V2, que tampoco lo usaba</param>
        /// <param name="contactos">Define si se devuelven los contactos (S/N)</param>
        /// <param name="unidadesdecliente">Define si se devuelven las unidades (S/N)</param>
        /// <param name="tiposDeCliente">Permite filtrar la busqueda por tipo de cliente</param>
        /// <param name="canalesDeVenta">Permite filtrar la busqueda por canal de venta</param>
        /// <param name="formasDePago">Permite filtrar la busqueda por formas de pago</param>
        /// <param name="condicionesIVA">Permite filtrar la busqueda por condiciones de IVA</param>
        /// <param name="ordenarPor">Define por que campo se ordena el listado</param>
        /// <param name="direccion">Define el tipo de orden del listado</param>
        [HttpGet("GetSearchResults")]
        public ActionResult<cResponseClientes> GetSearchResults(string? expresion = null,
                                                                string buscardatosdecontacto = "N",
                                                                int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                                int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                                string sucursales = "N",
                                                                string preciosespeciales = "N",
                                                                string fechamodificaciones = "",
                                                                string contactos = "N",
                                                                string unidadesdecliente = "N",
                                                                string? tiposDeCliente = null,
                                                                string? canalesDeVenta = null,
                                                                string? formasDePago = null,
                                                                string? condicionesIVA = null,
                                                                string? ordenarPor = null,
                                                                string? direccion = null)
            => Responder(() => ClientesMgr.GetSearchResults(expresion,
                                                            buscardatosdecontacto,
                                                            pageNumber,
                                                            pageSize,
                                                            sucursales,
                                                            preciosespeciales,
                                                            contactos,
                                                            unidadesdecliente,
                                                            tiposDeCliente,
                                                            canalesDeVenta,
                                                            formasDePago,
                                                            condicionesIVA,
                                                            ordenarPor,
                                                            direccion));

        /// <summary>
        /// Permite crear clientes
        /// </summary>
        [HttpPost("Create")]
        public ActionResult<cResponseClientes> Create([FromBody] List<cCliente>? oCliente = null)
            => Responder(() => ClientesMgr.Save(oCliente, true));

        /// <summary>
        /// Permite actualizar clientes (todos sus datos)
        /// </summary>
        [HttpPut("Update")]
        public ActionResult<cResponseClientes> Update([FromBody] List<cCliente>? oCliente = null)
            => Responder(() => ClientesMgr.Save(oCliente, false));

        /// <summary>
        /// Permite actualizar solamente determinados datos de clientes ya cargados
        /// </summary>
        [HttpPatch("update-parcial")]
        public ActionResult<cResponseSaveClientes> UpdatePatch([FromBody] List<cCliente>? oCliente = null)
            => Responder(() => ClientesMgr.Save(oCliente));

        /// <summary>
        /// Obtiene el listado de saldos de cada cliente
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de elementos por pagina</param>
        /// <param name="clientes">Permite filtrar determinados clientes (separados por coma)</param>
        /// <param name="tiposDeClienteID">Permite filtrar por tipo de cliente</param>
        /// <param name="fechaHasta">Permite obtener el saldo hasta determinada fecha</param>
        /// <param name="incluirRemitosPendientes">Define si se incluyen los remitos pendientes en el saldo (S/N)</param>
        /// <param name="incluirComprobanteMasAntiguo">Define si se incluye el comprobante mas antiguo (S/N)</param>
        /// <param name="incluirChequesEnCartera">Define si se incluyen los cheques en cartera (S/N)</param>
        [HttpGet("GetSaldos")]
        public ActionResult<cResponseSaldosClientes> GetSaldos(int pageNumber = (int)pPaginacionValoresPredeterminados.pPageNumber,
                                                               int pageSize = (int)pPaginacionValoresPredeterminados.pPageSize,
                                                               string? clientes = null,
                                                               string? tiposDeClienteID = null,
                                                               string? fechaHasta = null,
                                                               string incluirRemitosPendientes = "",
                                                               string incluirComprobanteMasAntiguo = "",
                                                               string incluirChequesEnCartera = "")
            => Responder(() => ClientesMgr.GetSaldos(pageNumber,
                                                     pageSize,
                                                     strClientes: clientes,
                                                     strTiposDeClienteID: tiposDeClienteID,
                                                     fechaHasta: fechaHasta,
                                                     blnIncluirRemitosPendientes: (incluirRemitosPendientes == "S"),
                                                     blnIncluirComprobanteMasAntiguo: (incluirComprobanteMasAntiguo == "S"),
                                                     blnIncluirChequesEnCartera: (incluirChequesEnCartera == "S")));
    }
}
