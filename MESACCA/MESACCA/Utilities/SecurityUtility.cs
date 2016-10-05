using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MESACCA
{
    public static class SecurityUtility
    {
        public static string ParseSQL(string input)
        {
            input.Replace("'","\\'").Replace("\\","\\\\");
            return input;
        }

    }
}