using GESI.CORE.V3.BLL;
using GESI.CORE.V3.BO;
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
        (bool blEncontradoEnHabilitacionesAPI,int EmpresaID, int SucursalID)? ObtenerEmpresaYSucursalPorDefecto(string usuarioID);

        /// <summary>El usuario puede operar en la empresa/sucursal pedida por headers.</summary>
        bool UsuarioPuedeOperar(string usuarioID, int empresaID, int sucursalID);
    }

    internal sealed class AccesoApiServicio(IAPILoginBLL apiLogin,IAPIHabilitacionesBLL apiHabilitaciones) : IAccesoApiServicio
    {
        public bool TokenHabilitado(string token) =>
            !string.IsNullOrEmpty(token)
            && apiLogin.GetItem(token) is { Count: > 0 };   // V2 por ahora

        public (bool blEncontradoEnHabilitacionesAPI, int EmpresaID, int SucursalID)? ObtenerEmpresaYSucursalPorDefecto(string usuarioID)
        {
            // Reemplazar por la lógica real de SetearMgrAPIParaMiddleware
            bool blEncontrado = false;
            int intEmpresaID = 1;
            int intSucursalID = 1;
            List<cAPIHabilitaciones>? lstHabilitacionesAPI = apiHabilitaciones.GetList(usuarioID);
            if (lstHabilitacionesAPI?.Count > 0) {
                foreach (cAPIHabilitaciones oHabilitacionAPI in lstHabilitacionesAPI)
                {
                    if (oHabilitacionAPI.TipoDeAPI.Equals("LEER_MAESTROS")) // VERIFICA SI ES DE TIPO LEER_MAESTROS
                    {
                        intEmpresaID = oHabilitacionAPI.EmpresaID;                 
                        intSucursalID = oHabilitacionAPI.SucursalID;
                        blEncontrado = true;
                    }

                }
            }

            return (blEncontrado,intEmpresaID,intSucursalID);
        }

        public bool UsuarioPuedeOperar(string usuarioID, int empresaID, int sucursalID)
        {
            // Reemplazar por la validación real de AnalizarEncabezadoSolicitud
            return true;
        }
    }
}
