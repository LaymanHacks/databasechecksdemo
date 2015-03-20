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
            var testResults = dbAccessor.RunChecks(dbToCheck, checks, appVersion);

            foreach(var result in testResults)
            {
                Console.WriteLine(string.Format("{0} - {1} ", result.Pass ? "pass" : "fail", result.Name));
            }

            Console.ReadLine(); 
        }
    }
}
