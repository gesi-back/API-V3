using GESI.CORE.API.BO.V3.CORE;
using GESI.CORE.V3.BLL;
using GESI.CORE.V3.BO;
using System.Diagnostics;

namespace GESI.CORE.API.BLL.V3.CORE
{
    public interface IConfiguracionesServicio
    {
        cConfiguraciones GetConfiguraciones(string grupoID = "", string seccionID = "", string itemID = "");
    }

    /// <summary>
    /// Migrado de la V2 (Core.BLL.ConfiguracionesMgr estático). Hoy lo usa Stock/Configuraciones con el grupo "stock".
    /// </summary>
    internal sealed class ConfiguracionesServicio(
        IConfiguracionBaseBLL configuracionBaseBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IConfiguracionesServicio
    {
        /// <summary>
        /// Devuelve las configuraciones del grupo agrupadas por sección.
        /// Igual que la V2: solo se devuelven las secciones de comprobantes de la empresa de la sesión ("comprobante_{EmpresaID}_...").
        /// </summary>
        public cConfiguraciones GetConfiguraciones(string grupoID = "", string seccionID = "", string itemID = "")
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            cConfiguraciones oConfiguraciones = new cConfiguraciones();

            List<cConfiguracionBase>? lstConfiguraciones = configuracionBaseBLL.GetList(grupoID, seccionID, itemID);

            string strPrefijoEmpresa = "comprobante_" + Sesion.EmpresaID + "_";

            oConfiguraciones.success = true;
            oConfiguraciones.grupo = grupoID;
            // Igual que la V2: si el Core no devuelve nada, secciones queda en null
            if (lstConfiguraciones != null)
            {
                oConfiguraciones.secciones = new Dictionary<string, List<SeccionItem>>();

                foreach (cConfiguracionBase oConfiguracion in lstConfiguraciones)
                {
                    string strSeccion = oConfiguracion.SeccionID.ToLower();

                    if (!strSeccion.StartsWith(strPrefijoEmpresa))
                        continue;   // configuraciones de otra empresa o que no son de comprobantes

                    if (!oConfiguraciones.secciones.ContainsKey(strSeccion))
                        oConfiguraciones.secciones.Add(strSeccion, new List<SeccionItem>());

                    oConfiguraciones.secciones[strSeccion].Add(new SeccionItem(oConfiguracion));
                }
            }

            LoguearInfo("Success Configuraciones GetList Grupo:" + grupoID + " Seccion:" + seccionID + " Item:" + itemID,
                        Endpoint.Stock.Configuraciones("GetList"), stopwatch, oConfiguraciones.secciones?.Count ?? 0, 1, 0);

            return oConfiguraciones;
        }
    }
}
