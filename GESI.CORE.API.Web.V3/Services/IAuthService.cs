namespace GESI.CORE.API.Web.V3.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateLogin(string? username, string? password);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaActual"></param>
        /// <param name="username"></param>
        /// <param name="tiempoValidez"></param>
        /// <returns></returns>
        string GenerateToken(DateTime fechaActual, string? username, TimeSpan tiempoValidez);
    }
}
