using Microsoft.Extensions.Configuration;


namespace RoboSlogan
{
    class AppSettings : IAppSettings
    {

        private readonly IConfiguration _config;
        public AppSettings(IConfiguration config)
        {
            _config = config;
        }

        public string Token => _config.GetValue<string>("AppSettings:Token");
        public char SpecialChar => _config.GetValue<char>("AppSettings:SpecialChar");
        string IAppSettings.ApplicationLog => _config.GetValue<string>("AppSettings:ApplicationLog");
    }
}
