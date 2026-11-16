using GESI.CORE.V3.BLL;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BLL.V3
{
    public sealed class ApiSettings
    {
        public const string Seccion = "Api";
        public int TamanoPaginaMaximo { get; set; } = 1000;
        public int? EmpresaPorDefecto { get; set; }
        public int? SucursalPorDefecto { get; set; }
    }

    public interface IApiSessionManager
    {
        int EmpresaID { get; }
        int SucursalID { get; }
        string Usuario { get; }
        string MetodoAutenticacion { get; }   // "ApiKey" o "Bearer"
        string CorrelationId { get; }
        int TamanoPagina { get; }
        string RemoteIPAddress { get; }
        string Forwarded { get; }
        string UserAgent { get; }
        bool Iniciada { get; }
        string? CategoriasIDs { get; }

        string CostosXProveedor { get; }

        string? EstadoProductos { get; }

        int[]? Almacenes { get; }

        string? ReleePreciosAlImportarComprobantes { get; } 

        string? TiposDeClientes { get; } 

        bool UsuarioExterno { get; }

        void Iniciar(int empresaID, int sucursalID, string usuario, string metodoAutenticacion,
                     string? correlationId, string? remoteIP, string? forwarded, string? userAgent,string? categoriasIDs, string? costosxproveedor, string? estadoProductos, string releePreciosAlImportarComprobantes,string? almacenes,
                     string? tiposDeClientes = null, bool usuarioExterno = false);
    }

    internal sealed class ApiSessionManager(SessionManagerAccessor accessor, IOptions<ApiSettings> config) : IApiSessionManager
    {
        private SessionManager Core =>
            accessor.Current ?? throw new InvalidOperationException("No hay sesión iniciada para este request.");

        public int EmpresaID => Core.EmpresaID;
        public int SucursalID => Core.SucursalID;
        public string Usuario { get; private set; } = "";
        public string MetodoAutenticacion { get; private set; } = "";
        public string CorrelationId { get; private set; } = "";
        public int TamanoPagina { get; } = config.Value.TamanoPaginaMaximo;        
        public string RemoteIPAddress { get; private set; } = "";
        public string Forwarded { get; private set; } = "";
        public string UserAgent { get; private set; } = "";
        public bool Iniciada => accessor.Current is not null;
        public string? CategoriasIDs { get; private set; } = null;

        public string CostosXProveedor { get; private set; } = "";

        public string? EstadoProductos { get; private set; } = null;

        public int[]? Almacenes { get; private set; } = null;

        public string? ReleePreciosAlImportarComprobantes { get; private set; } = null;

        public string? TiposDeClientes { get; private set; } = null;

        public bool UsuarioExterno { get; private set;  } = false;




        public void Iniciar(int empresaID, int sucursalID, string usuario, string metodoAutenticacion,
                            string? correlationId, string? remoteIP, string? forwarded, string? userAgent,string? categoriasIDs,string? costosxproveedor,string? estadoProductos, string releePreciosAlImportarComprobantes, string? almacenes,
                            string? tiposDeClientes = null, bool usuarioExterno = false)
        {
            accessor.Current = new SessionManager
            {
                EmpresaID = empresaID,
                SucursalID = sucursalID,
                UserID = usuario,    // como hacía MiAPISessionMgr.SessionMgr.UserID
                UserName = usuario,
            };

            Usuario = usuario;
            MetodoAutenticacion = metodoAutenticacion;
            CorrelationId = correlationId ?? "";
            RemoteIPAddress = remoteIP ?? "";
            Forwarded = forwarded ?? "";
            UserAgent = userAgent ?? "";
            CategoriasIDs = categoriasIDs;
            // Restricción de la habilitación: la integración solo ve/graba clientes de estos tipos (null o vacío = todos)
            TiposDeClientes = string.IsNullOrWhiteSpace(tiposDeClientes) ? null : tiposDeClientes;
            // Igual que la V2: a un usuario externo se le exige CodigoDeImportacion al grabar comprobantes
            UsuarioExterno = usuarioExterno;
            CostosXProveedor = costosxproveedor;
            EstadoProductos = estadoProductos;
            ReleePreciosAlImportarComprobantes = releePreciosAlImportarComprobantes;

            if(almacenes != null)
                Almacenes = almacenes
                                .Split(',')
                                .Select(int.Parse)
                                .ToArray(); ;
                                    }
    }
}
