using GESI.CORE.V3.BLL;
using GESI.CORE.V3.BO;
using GESI.ERP.Core.V3.BLL;
using GESI.ERP.Core.V3.BO;
using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BLL.V3
{
    
    public interface IAccesoApiServicio
    {
        /// <summary>El token existe y está activo en la tabla de tokens (ex APILoginManager.GetItem).</summary>
        bool TokenHabilitado(string token);

        /// <summary>Empresa y sucursal por defecto del usuario (ex SetearMgrAPIParaMiddleware).</summary>
        cAPIHabilitaciones? ObtenerHabilitacionAPIDelUsuario(string usuarioID);

        /// <summary>El usuario puede operar en la empresa/sucursal pedida por headers.</summary>
        bool UsuarioPuedeOperar(string usuarioID, int empresaID, int sucursalID);
    }

    internal sealed class AccesoApiServicio(IAPILoginBLL apiLogin,IAPIHabilitacionesBLL apiHabilitaciones,IEmpresasBLL empresasBLL) : IAccesoApiServicio
    {
        public static string TipoDeAPI = "LEER_MAESTROS";
        public bool TokenHabilitado(string token) =>
            !string.IsNullOrEmpty(token)
            && apiLogin.GetItem(token) is { Count: > 0 };   // V2 por ahora

        public cAPIHabilitaciones? ObtenerHabilitacionAPIDelUsuario(string usuarioID)
        {
            // Reemplazar por la lógica real de SetearMgrAPIParaMiddleware
            cAPIHabilitaciones? oAPIHabilitacion = null;
            List<cAPIHabilitaciones>? lstHabilitacionesAPI = apiHabilitaciones.GetList(usuarioID);

            if (lstHabilitacionesAPI?.Count > 0)
                oAPIHabilitacion = lstHabilitacionesAPI.FirstOrDefault(x => x.TipoDeAPI.Equals(TipoDeAPI));            

            return oAPIHabilitacion;
        }


        public bool UsuarioPuedeOperar(string usuarioID, int empresaID, int sucursalID)
        {
            List<cEmpresa> lstEmpresasHabilitadas = empresasBLL.GetListByUsuario(usuarioID);

            cEmpresa? oEmpresa = lstEmpresasHabilitadas.FirstOrDefault(x => x.EmpresaID == empresaID);

            if (oEmpresa == null)
                return false;
            else
            {
                cSucursal? oSucursal = oEmpresa.LstSucursales?.FirstOrDefault(x => x.SucursalID == sucursalID);

                if (oSucursal == null)
                    return false;
                else return true;
            }
        }
    }
}
