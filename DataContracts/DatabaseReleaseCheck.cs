using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace databasechecksdemo.DataContracts
{
    class DatabaseReleaseCheck
    {
        public long Id { get; set; }
        public string SqlCheck { get; set; }
        public string Expected { get; set; }
        public string Operator { get; set; }
        public string Severity { get; set; }
        public string Name { get; set; }
        public string Actual { get; set; }

        public bool Pass { get; set; }

        public override string ToString()
        {
            var actual = (Actual == null) ? string.Empty : Actual;
            var dbReleaseCheckStr = string.Format(@"{0}_{1} : {2} {3} {4}", Id, Name, Expected, Operator, actual);
            return dbReleaseCheckStr;
        }
    }
}
