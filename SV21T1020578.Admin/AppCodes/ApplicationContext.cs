using Newtonsoft.Json;

namespace SV21T1020578
{
    public static class ApplicationContext
    {
        private static IHttpContextAccessor? httpContextAccessor;
        private static IWebHostEnvironment? hostEnvironment;

        public static void Configure(IHttpContextAccessor context, IWebHostEnvironment enviroment)
        {
            ApplicationContext.httpContextAccessor = context ?? throw new ArgumentNullException();
            ApplicationContext.hostEnvironment = enviroment ?? throw new ArgumentNullException();
        }
        /// <summary>
        /// HttpContext
        /// </summary>
        public static HttpContext? HttpContext => httpContextAccessor?.HttpContext;
        /// <summary>
        /// HostEnviroment
        /// </summary>
        public static IWebHostEnvironment? HostEnviroment => hostEnvironment;
        /// <summary>
        /// Đường dẫn vật lý đến thư mục lưu các static file (thư mục wwwroot)
        /// </summary>
        public static string WebRootPath => hostEnvironment?.WebRootPath ?? string.Empty;
        /// <summary>
        /// Đường dẫn vật lý đến thư mục lưu ứng dụng Web
        /// </summary>
        public static string ContentRootPath => hostEnvironment?.ContentRootPath ?? string.Empty;


        /// <summary>
        /// Ghi dữ liệu vào session
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetSessionData(string key, object value)
        {
            try
            {

                string sValue = JsonConvert.SerializeObject(value);
                if (!string.IsNullOrEmpty(sValue))
                {
                    httpContextAccessor?.HttpContext?.Session.SetString(key, sValue);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Đọc dữ liệu từ session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T? GetSessionData<T>(string key) where T : class
        {
            try
            {
                string sValue = httpContextAccessor?.HttpContext?.Session.GetString(key) ?? "";
                if (!string.IsNullOrEmpty(sValue))
                {
                    return JsonConvert.DeserializeObject<T>(sValue);
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
