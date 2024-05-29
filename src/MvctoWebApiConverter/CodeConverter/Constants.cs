using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter
{
   public class Constants
    {
        public string HttpAction = AppSettingKey.GetAppSettingKey("HttpAction");     
        public string WebMvcUsing = AppSettingKey.GetAppSettingKey("WebMvcUsing");
        public string ApiController = AppSettingKey.GetAppSettingKey("ApiController");
        public string BaseController = AppSettingKey.GetAppSettingKey("BaseController");
        public string SystemWebHttp = AppSettingKey.GetAppSettingKey("SystemWebHttp");
        public string Controller = AppSettingKey.GetAppSettingKey("Controller");
        public string RoutePrefix = AppSettingKey.GetAppSettingKey("RoutePrefix");
        public string ControllerRoot = AppSettingKey.GetAppSettingKey("ControllerRoot");
        public string IHttpActionResult = AppSettingKey.GetAppSettingKey("IHttpActionResult");
        public string Route = AppSettingKey.GetAppSettingKey("Route");
        public string HttpPost = AppSettingKey.GetAppSettingKey("HttpPost");
        public string HttpPut = AppSettingKey.GetAppSettingKey("HttpPut");
        public string HttpDelete = AppSettingKey.GetAppSettingKey("HTTPDELETE");
        public string httpAction = AppSettingKey.GetAppSettingKey("httpAction");
        public string HttpGet = AppSettingKey.GetAppSettingKey("HttpGet");
        public string View = AppSettingKey.GetAppSettingKey("View");
        public string MethodOk = AppSettingKey.GetAppSettingKey("OkMethod");
        public string HttpNotFound = AppSettingKey.GetAppSettingKey("HttpNotFound");
        public string NotFound = AppSettingKey.GetAppSettingKey("NotFoundMethod");
        public string Redirect = AppSettingKey.GetAppSettingKey("Redirect");
        public string RedirectToRoute = AppSettingKey.GetAppSettingKey("RedirectToRoute");
        public string Json = AppSettingKey.GetAppSettingKey("Json");
        public string ValidateAntiForgeryToken = AppSettingKey.GetAppSettingKey("ValidateAntiForgeryToken");
        public string Folderstomove = AppSettingKey.GetAppSettingKey("Folderstomove");
        public string Controllerkey = AppSettingKey.GetAppSettingKey("ControllerIdentifiers");
        public string SourcePath = AppSettingKey.GetAppSettingKey("SourcePath");
        public string Destinationpath = AppSettingKey.GetAppSettingKey("Destinationpath");
        public string filestoexclude = AppSettingKey.GetAppSettingKey("filestoexclude");
        public string _redisCacheService = AppSettingKey.GetAppSettingKey("_redisCacheService");
        public string RedisCacheService = AppSettingKey.GetAppSettingKey("RedisCacheService");
        public string RedisCacheServices = AppSettingKey.GetAppSettingKey("RedisCacheServices");
        public string RedisCacheServiceClass = AppSettingKey.GetAppSettingKey("RedisCacheService.cs");
        public string MoveAllorCopySelected = AppSettingKey.GetAppSettingKey("MoveRequiredFilesorCopySelected");
        public string MoveAll = "MoveRequired";
        public string ServiceClassString =
             @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StackExchange.Redis;
using System.Configuration;

namespace WebAPITest001.Services
{
    public class RedisCacheService
    {
        private static Lazy<ConnectionMultiplexer> _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string connectionString = GetAppSettingKey(""RedisCacheUrl"");
            return ConnectionMultiplexer.Connect(connectionString);
        });

        private static IDatabase Cache => _lazyConnection.Value.GetDatabase();

        public void Set(string key, string value, TimeSpan? expiry = null)
        {
            Cache.StringSet(key, value, expiry);
        }

        public string Get(string key)
        {
            return Cache.StringGet(key);
        }

        public bool Remove(string key)
        {
            return Cache.KeyDelete(key);
        }
    }
}";



      

    }


    public static class AppSettingKey
    {
        public static string GetAppSettingKey(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            return value ?? throw new ConfigurationErrorsException($"Configuration value for key '{key}' not found.");
        }
    }

}
