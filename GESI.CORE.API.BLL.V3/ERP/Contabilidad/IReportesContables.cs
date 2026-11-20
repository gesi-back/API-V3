using GESI.Contabilidad.V3.BLL;
using GESI.Contabilidad.V3.BO;
using GESI.CORE.API.BLL.V3.CORE;
using GESI.CORE.API.BO.V3.API;
using GESI.CORE.API.BO.V3.ERP.Contabilidad;
using System;
using System.Collections.Generic;
using System.Text;

namespace GESI.CORE.API.BLL.V3.ERP.Contabilidad
{
    public interface IReportesContables
    {
        cResponseLibroDiario GetLibroDiario(int ejercicioID,
            DateTime? desde = null,
            DateTime? hasta = null);

        cResponseMayores GetMayor(int ejercicioID,
             int pageNumber, int pageSize,
             DateTime? desde = null,
             DateTime? hasta = null,
             string? cuentasIDs = null);

        cResponseBalanceDeSumasYSaldos GetBalanceDeSumasYSaldos(int ejercicioID,
             DateTime? desde = null,
             DateTime? hasta = null,
             string incluirCuentasTotalizadoras = "N",
             string? asientosExcluidos = null,
             int? asientoDeAperturaID = null);

        cResponseBalanceGeneral GetBalanceGeneral(int ejercicioID,
             DateTime? hasta = null,
             string? cuentasRaiz = null,
             int? nivelMaximo = null,
             string ocultarCuentasSinSaldo = "S",
             string? asientosExcluidos = null);
    }

    internal sealed class ReportesContables(
        IReportesContablesBLL reportesContablesBLL,
        IAPIHelper apiHelper,
        IApiSessionManager sesion,
        IApiLogger apiLogger) : ServicioBase(apiHelper, sesion, apiLogger), IReportesContables
    {

        public cResponseLibroDiario GetLibroDiario(int ejercicioID, 
            DateTime? desde = null , 
            DateTime? hasta = null)
        {
            cResponseLibroDiario oResponse = new cResponseLibroDiario();

            try
            {
                if (ejercicioID <= 0)
                    throw new cCodigoNoDetalladoException("No se detallo el ejercicioID");

                if (ejercicioID > byte.MaxValue)
                    throw new ArgumentException("El ejercicioID no es valido");

                cLibroDiario oLibroDiario = reportesContablesBLL.GetLibroDiario((byte)ejercicioID, desde, hasta);
                oResponse.librodiario = oLibroDiario;
                oResponse.paginacion = new cPaginacion(1, 1, 1, 1, 1);

                if (oLibroDiario == null)
                    throw new cDatosNoEncontradosException("No se encontraron datos para los parametros seleccionados");

                oResponse.success = true;
            }
            catch(cDatosNoEncontradosException)
            {
                oResponse.error = new cError((int)cCodigosError.cDatosNoEncontrados, "No se encontró el libro diario", ApiErrors.errDatosNoEncontrados);
            }
            catch(cCodigoNoDetalladoException)
            {
                oResponse.error = new cError((int)cCodigosError.cCodigoDetalladoEnLaSolicitud, "No se detalló el ejercicioID", ApiErrors.errJsonInvalido);
            }
            return oResponse;
        }

        public cResponseMayores GetMayor(int ejercicioID,
            int pageNumber, int pageSize,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? cuentasIDs = null)
        {
            cResponseMayores oResponse = new cResponseMayores();

            try
            {
                if (ejercicioID <= 0)
                    throw new cCodigoNoDetalladoException("No se detallo el ejercicioID");

                if (ejercicioID > byte.MaxValue)
                    throw new ArgumentException("El ejercicioID no es valido");

                List<string>? lstCuentas = null;
                if(cuentasIDs?.Length > 0)
                {
                    lstCuentas = new List<string>();
                    lstCuentas = cuentasIDs.Split(',').ToList();
                }

                List<cMayorCuenta> lstMayores = reportesContablesBLL.GetMayor((byte)ejercicioID, desde, hasta, lstCuentas);

                if (lstMayores == null || lstMayores.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron mayores para las cuentas seleccionadas");

                int cantidadElementos = lstMayores.Count;

                // Igual que los demás GetList: se respeta el tamaño máximo de página de la habilitación
                // y se evita la división por cero con pageSize = 0
                pageSize = Math.Max(ApiHelper.DevolverTamanoPaginacion(Sesion.TamanoPagina, pageSize), 1);
                pageNumber = Math.Max(pageNumber, 1);

                lstMayores = lstMayores.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (lstMayores.Count == 0)
                    throw new cIntervalException(new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber));

                oResponse.mayores = lstMayores;
                oResponse.success = true;
                oResponse.paginacion = new cPaginacion(cantidadElementos, (int)Math.Ceiling((double)cantidadElementos / pageSize), pageNumber, pageSize, pageNumber);
            }
            catch (cIntervalException oIntervalo)
            {
                oResponse.paginacion = oIntervalo._paginacion;
                oResponse.success = true;
                oResponse.error = new cError((int)cCodigosError.cPaginacionExcedida, "Paginación Excedida", ApiErrors.errPaginacionExcedida);
            }
            catch (cDatosNoEncontradosException)
            {
                oResponse.error = new cError((int)cCodigosError.cDatosNoEncontrados, "No se encontraron mayores para las cuentas seleccionadas", ApiErrors.errDatosNoEncontrados);
            }
            catch (cCodigoNoDetalladoException)
            {
                oResponse.error = new cError((int)cCodigosError.cCodigoDetalladoEnLaSolicitud, "No se detalló el ejercicioID", ApiErrors.errJsonInvalido);
            }
            return oResponse;
        }

        public cResponseBalanceDeSumasYSaldos GetBalanceDeSumasYSaldos(int ejercicioID,
            DateTime? desde = null,
            DateTime? hasta = null,
            string incluirCuentasTotalizadoras = "N",
            string? asientosExcluidos = null,
            int? asientoDeAperturaID = null)
        {
            cResponseBalanceDeSumasYSaldos oResponse = new cResponseBalanceDeSumasYSaldos();

            try
            {
                if (ejercicioID <= 0)
                    throw new cCodigoNoDetalladoException("No se detallo el ejercicioID");

                if (ejercicioID > byte.MaxValue)
                    throw new ArgumentException("El ejercicioID no es valido");

                // "1,2,3" → [1, 2, 3]. Un código no numérico da FormatException (400)
                List<int>? lstAsientosExcluidos = null;
                if (asientosExcluidos?.Length > 0)
                {
                    lstAsientosExcluidos = asientosExcluidos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                            .Select(int.Parse)
                                                            .ToList();
                }

                cBalanceDeSumasYSaldos oBalance = reportesContablesBLL.GetBalanceDeSumasYSaldos((byte)ejercicioID, desde, hasta,
                                                                                               incluirCuentasTotalizadoras == "S",
                                                                                               lstAsientosExcluidos,
                                                                                               asientoDeAperturaID);

                if (oBalance == null || oBalance.Renglones == null || oBalance.Renglones.Count == 0)
                    throw new cDatosNoEncontradosException("No se encontraron datos para los parametros seleccionados");

                oResponse.balanceDeSumasYSaldos = oBalance;
                oResponse.success = true;
                oResponse.paginacion = new cPaginacion(1, 1, 1, 1, 1);
            }
            catch (cDatosNoEncontradosException)
            {
                oResponse.error = new cError((int)cCodigosError.cDatosNoEncontrados, "No se encontró el balance de sumas y saldos", ApiErrors.errDatosNoEncontrados);
            }
            catch (cCodigoNoDetalladoException)
            {
                oResponse.error = new cError((int)cCodigosError.cCodigoDetalladoEnLaSolicitud, "No se detalló el ejercicioID", ApiErrors.errJsonInvalido);
            }
            return oResponse;
        }

        public cResponseBalanceGeneral GetBalanceGeneral(int ejercicioID,
            DateTime? hasta = null,
            string? cuentasRaiz = null,
            int? nivelMaximo = null,
            string ocultarCuentasSinSaldo = "S",
            string? asientosExcluidos = null)
        {
            cResponseBalanceGeneral oResponse = new cResponseBalanceGeneral();

            try
            {
                if (ejercicioID <= 0)
                    throw new cCodigoNoDetalladoException("No se detallo el ejercicioID");

                if (ejercicioID > byte.MaxValue)
                    throw new ArgumentException("El ejercicioID no es valido");

                List<string>? lstCuentasRaiz = null;
                if (cuentasRaiz?.Length > 0)
                {
                    lstCuentasRaiz = cuentasRaiz.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }

                // "1,2,3" → [1, 2, 3]. Un código no numérico da FormatException (400)
                List<int>? lstAsientosExcluidos = null;
                if (asientosExcluidos?.Length > 0)
                {
                    lstAsientosExcluidos = asientosExcluidos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                            .Select(int.Parse)
                                                            .ToList();
                }

                cBalanceGeneral oBalance = reportesContablesBLL.GetBalanceGeneral((byte)ejercicioID, hasta,
                                                                                 lstCuentasRaiz,
                                                                                 nivelMaximo,
                                                                                 ocultarCuentasSinSaldo != "N",
                                                                                 lstAsientosExcluidos);

                if (oBalance == null || ((oBalance.Rubros == null || oBalance.Rubros.Count == 0) && (oBalance.Lineas == null || oBalance.Lineas.Count == 0)))
                    throw new cDatosNoEncontradosException("No se encontraron datos para los parametros seleccionados");

                oResponse.balanceGeneral = oBalance;
                oResponse.success = true;
                oResponse.paginacion = new cPaginacion(1, 1, 1, 1, 1);
            }
            catch (cDatosNoEncontradosException)
            {
                oResponse.error = new cError((int)cCodigosError.cDatosNoEncontrados, "No se encontró el balance general", ApiErrors.errDatosNoEncontrados);
            }
            catch (cCodigoNoDetalladoException)
            {
                oResponse.error = new cError((int)cCodigosError.cCodigoDetalladoEnLaSolicitud, "No se detalló el ejercicioID", ApiErrors.errJsonInvalido);
            }
            return oResponse;
        }

    }
}
