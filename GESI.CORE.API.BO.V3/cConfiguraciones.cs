using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BO.V3
{
    /// <summary>
    /// Configuraciones devueltas por el ERP
    /// </summary>
    public class cConfiguraciones : cResponse
    {
        /// <summary>
        /// Grupo de configuraciones
        /// </summary>
        public string? grupo { get; set; }

        /// <summary>
        /// clave = nombre de la sección (configuracion, preferencias, etc)
        /// </summary>
        public Dictionary<string, List<SeccionItem>>? secciones { get; set; }
    }


    /// <summary>
    /// Seccion a la que pertenece la configuracion
    /// </summary>
    public class SeccionItem
    {
       

        /// <summary>
        /// Constructor con parametros
        /// </summary>
        /// <param name="oConfig"></param>
        public SeccionItem(GESI.CORE.V3.BO.cConfiguracionBase oConfig)
        {
            this.item = oConfig.ItemID;
            this.valor = oConfig.Valor;
            this.editable = oConfig.EditablePorElUsuario;
        }

        /// <summary>
        /// Tipo de item
        /// </summary>
        public string item { get; set; }
        /// <summary>
        /// Valor del item
        /// </summary>
        public string valor { get; set; }
        /// <summary>
        /// Define si es editable
        /// </summary>
        public bool editable { get; set; }
    }
}
