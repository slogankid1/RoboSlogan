using System;
using System.Collections.Generic;
using System.Text;

namespace RoboSlogan
{
    public interface IAppSettings
    {
        public string Token { get; }
        public char SpecialChar { get; }
        public string ApplicationLog { get; }
    }
}
