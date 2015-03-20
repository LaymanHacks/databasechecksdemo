using databasechecksdemo.DataContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace databasechecksdemo
{
    class DatabaseAccessor
    {
        public List<DatabaseReleaseCheck> GetDatabaseChecks(bool isProduction, long appVersion)
        {
            List<DatabaseReleaseCheck> checks = new List<DatabaseReleaseCheck>();
            //var factory = new ProxyFactory();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CheckDatabase"].ToString()))
            {
                conn.Open();
                var sql = @"select * from dbo.DatabaseReleaseChecks 
  where (AppVersionStart is null or AppVersionStart < @AppVersion)
	and (AppVersionEnd is null or AppVersionEnd > @AppVersion)
	and (IsProduction is null or IsProduction = @IsProduction)
    and IsDeleted = '0'";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppVersion", appVersion);
                    cmd.Parameters.AddWithValue("@IsProduction", isProduction);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            checks.Add(CheckReader(reader));
                        }
                    }
                }
            }

            return checks;
        }

        public Dictionary<string, List<DatabaseReleaseCheck>> RunChecks(string connectionString, List<DatabaseReleaseCheck> checks, long appVersion)
        {
            Dictionary<string, List<DatabaseReleaseCheck>> result = new Dictionary<string, List<DatabaseReleaseCheck>>();

            foreach (var chk in checks)
            {
                string actual = "";
                bool pass = false;
                try
                {
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(chk.SqlCheck, conn))
                        {
                            cmd.CommandTimeout = 600;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader[0] != null && reader[0] != DBNull.Value)
                                        actual = reader[0].ToString();
                                    else
                                        actual = "NULL";
                                }
                            }
                        }
                    }
                    pass = CheckCompare(chk, actual, appVersion);
                    if (!pass)
                    {
                        chk.Actual = actual;
                        if (result.ContainsKey(chk.Severity))
                        {
                            List<DatabaseReleaseCheck> errList;
                            if (result.TryGetValue(chk.Severity, out errList))
                            {

                                errList.Add(chk);
                                result.Remove(chk.Severity);
                                result.Add(chk.Severity, errList);
                            }
                            else
                            {
                                var err = new List<DatabaseReleaseCheck>();
                                err.Add(chk);
                                result.Add(chk.Severity, err);
                            }
                        }
                        else
                        {
                            var err = new List<DatabaseReleaseCheck>();
                            err.Add(chk);
                            result.Add(chk.Severity, err);
                        }
                    }

                }
                catch (Exception e)
                {
                    pass = false;
                    chk.Actual = string.Format("Error occurred while trying to run check {0} : {1}", e.Message, e.StackTrace);
                    //result.Add("EXCEPTION", new List<DatabaseReleaseCheck>() { chk });
                    var severity = "EXCEPTION";
                    if (result.ContainsKey(severity))
                    {
                        List<DatabaseReleaseCheck> errList;
                        if (result.TryGetValue(severity, out errList))
                        {

                            errList.Add(chk);
                            result.Remove(severity);
                            result.Add(severity, errList);
                        }
                        else
                        {
                            var err = new List<DatabaseReleaseCheck>();
                            err.Add(chk);
                            result.Add(severity, err);
                        }
                    }
                    else
                    {
                        var err = new List<DatabaseReleaseCheck>();
                        err.Add(chk);
                        result.Add(severity, err);
                    }
                }

            }

            return result;
        }

        private static DatabaseReleaseCheck CheckReader(SqlDataReader reader)
        {
            DatabaseReleaseCheck check = new DatabaseReleaseCheck();

            check.Id = long.Parse(reader["Id"].ToString());
            check.SqlCheck = reader["SqlCheck"].ToString();
            check.Expected = reader["Expected"].ToString();
            check.Operator = reader["Operator"].ToString();
            check.Severity = reader["Severity"].ToString();
            check.Name = reader["Name"].ToString();

            return check;
        }

        private bool CheckCompare(DatabaseReleaseCheck check, string actual, long appVersion)
        {
            var expected = check.Expected;

            // only the = operator can compare strings
            if (check.Operator == "=")
            {
                return expected == actual;
            }

            // assume that we are comparing longs
            long act, exp;
            if (long.TryParse(actual, out act) && long.TryParse(expected, out exp))
            {
                switch (check.Operator)
                {
                    case "<":
                        return exp < act;
                    case "<=":
                        return exp <= act;
                    case ">":
                        return exp > act;
                    case ">=":
                        return exp >= act;
                    case "<>":
                        return exp != act;
                    default:
                        return exp == act;
                }
            }
            else
                return false;
        }
    }
}
