using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace databasechecksdemo
{
    class Program
    {
        static void Main(string[] args)
        {
            long appVersion = 100; 
            var dbAccessor = new DatabaseAccessor();
            var checks = dbAccessor.GetDatabaseChecks(false, appVersion);

            var dbToCheck = ConfigurationManager.ConnectionStrings["DatabaseToCheck"].ConnectionString;
            var errors = dbAccessor.RunChecks(dbToCheck, checks, appVersion);

        }
    }
}
