using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace HOWTOUSE.DAC
{
    public static class DapperConfig
    {
        public static void Configure()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }
    }
}
