using GESI.CORE.API.BO.V3;
using GESI.CORE.V3.BLL;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3
{
    public interface IPaisesServicio
    {
        cResponsePaises GetList(int pageNumber, int pageSize);
        cResponsePais GetItem(int paisID);
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.PaisesMgr estático con MiApiSessionManager estático).
    /// Los paises vienen del CORE (GESI.CORE.V3), no del ERP.
    /// </summary>
    internal sealed class PaisesServicio(
        IPaisesBLL paisesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IPaisesServicio
    {
        public cResponsePaises GetList(int pageNumber, int pageSize)
            => Paginar(new cResponsePaises(),
                       () => paisesBLL.GetList(),
                       pageNumber, pageSize,
                       endpoint: Endpoint.Maestros.Paises("GetList"),
                       nombreLog: "Paises GetList",
                       mensajeNoEncontrado: "No se encontraron paises",
                       asignarItems: (r, items) => r.paises = items);

        public cResponsePais GetItem(int paisID)
        {
            var stopwatch = Stopwatch.StartNew();

            if (paisID == 0)
                throw new cCodigoNoDetalladoException("No se especifico el paisID");   // → 400

            var pais = paisesBLL.GetItem(paisID)
                       ?? throw new cDatosNoEncontradosException("No se encontró el pais " + paisID);   // → 404

            LoguearInfo("Success Paises GetItem", Endpoint.Maestros.Paises("GetItem"), stopwatch, 1, 1, 1);

            return new cResponsePais
            {
                pais = pais,
                success = true,
                paginacion = new cPaginacion(1, 1, 1, 1, 1),
            };
        }
    }
}
