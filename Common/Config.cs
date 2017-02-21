using System.Configuration;
using System.Text;

namespace Common
{
    public static class Config
    {

        #region GetAppSettings(获取appSettings)

        /// <summary>
        /// 获取appSettings
        /// </summary>
        /// <param name="key">键名</param>
        public static string GetAppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString();
        }

        /// <summary>
        /// 获取appSettings
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static string GetAppSettings(string key, string def)
        {
            var setting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(setting))
            {
                return def;
            }
            return setting;
        }

        #endregion GetAppSettings(获取appSettings)

        #region GetConnectionString(获取连接字符串)

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="key">键名</param>
        public static string GetConnectionString(string key)
        {
            return ConfigurationManager.ConnectionStrings[key].ToString();
        }

        #endregion GetConnectionString(获取连接字符串)

        #region GetProviderName(获取数据提供程序名称)

        /// <summary>
        /// 获取数据提供程序名称
        /// </summary>
        /// <param name="key">键名</param>
        public static string GetProviderName(string key)
        {
            return ConfigurationManager.ConnectionStrings[key].ProviderName;
        }

        #endregion GetProviderName(获取数据提供程序名称)
    }
}