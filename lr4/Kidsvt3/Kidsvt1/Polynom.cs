using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kidsvt1
{
    class Polynom
    {
        public int XorsCount { get; set; }

        public Func<List<bool>, bool, bool> LogicOperation { get; set; }
    }
}
