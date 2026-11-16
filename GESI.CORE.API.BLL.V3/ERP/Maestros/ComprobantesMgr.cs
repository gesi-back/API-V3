using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Maestros;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.ERP.Maestros
{
    public interface IComprobantesServicio
    {
        cResponseComprobantes GetList(int comprobanteID, int claseDeComprobanteID, int pageNumber, int pageSize);
        cResponseComprobante GetItem(int comprobanteID, int claseDeComprobanteID);
        cResponseSaveComprobante Create(cComprobante? comprobante);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.ComprobantesMgr estático con _MIAPISessionMgr estático).
    /// La sesión, el BLL del Core y el logger llegan por inyección de dependencias.
    /// </summary>
    internal sealed class ComprobantesServicio(
        IComprobantesBLL comprobantesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IComprobantesServicio
    {
        public cResponseComprobantes GetList(int comprobanteID, int claseDeComprobanteID, int pageNumber, int pageSize)
            => Paginar(new cResponseComprobantes(),
                       // Igual que la V2: solo comprobantes activos ("A") de la sucursal de la sesión
                       () => comprobantesBLL.GetList(comprobanteID, claseDeComprobanteID, "A", Sesion.SucursalID),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Comprobantes("GetList"),
                       nombreLog: "Comprobantes GetList",
                       mensajeNoEncontrado: "No se encontraron comprobantes",
                       asignarItems: (r, items) => r.comprobantes = items);

        public cResponseComprobante GetItem(int comprobanteID, int claseDeComprobanteID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (comprobanteID <= 0)
                throw new cCodigoNoDetalladoException("No se especificó el comprobanteID");   // → 400

            // Igual que la V2: el GetItem no filtra por estado ni sucursal
            var comprobante = comprobantesBLL.GetList(comprobanteID, claseDeComprobanteID)?.FirstOrDefault()
                              ?? throw new cDatosNoEncontradosException("No se encontró el comprobante " + comprobanteID);   // → 404

            LoguearInfo("Success Comprobantes GetItem", Endpoint.Maestros.Comprobantes("GetItem"), stopwatch, 1, 1, 1);

            return new cResponseComprobante
            {
                comprobante = comprobante,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }

        public cResponseSaveComprobante Create(cComprobante? comprobante)
        {
            var stopwatch = Stopwatch.StartNew();

            if (comprobante is null)
                throw new cCodigoNoDetalladoException("No se informó el comprobante a crear");   // → 400

            // La empresa sale siempre de la sesión: el cliente no puede crear comprobantes en otra empresa
            comprobante.EmpresaID = (short)Sesion.EmpresaID;

            var (exito, mensaje) = comprobantesBLL.Save(comprobante, blnAlta: true);

            // El Core rechazó el comprobante (validación de negocio) → 400 con su mensaje
            if (!exito)
                throw new ArgumentException(string.IsNullOrWhiteSpace(mensaje) ? "No se pudo crear el comprobante" : mensaje);

            LoguearInfo("Success Comprobantes Create", Endpoint.Maestros.Comprobantes("Create"), stopwatch, 1, 1, 1);

            return new cResponseSaveComprobante { comprobante = comprobante, success = true };
        }
    }
}
