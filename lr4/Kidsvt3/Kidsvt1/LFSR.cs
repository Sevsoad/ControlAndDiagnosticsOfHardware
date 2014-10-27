using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kidsvt1
{
    public class LFSR
    {
        public List<bool> states;

        public LFSR()
        {
            states = new List<bool>(7);

            for (var i = 0; i < 7; i++)
                states.Add(true);
        }

        public LFSR(List<bool> inList)
        {
            if (inList.Count == 7)
                states = inList;
        }

        public void Iterate()
        {
            var xorValue = GetXorValue();
            bool tmp = states[0];
            var tmpArray = new List<bool>(states);

            tmpArray.RemoveAt(tmpArray.Count - 1);

            states[0] = xorValue;                
            states.RemoveRange(1, 6);
            states.AddRange(tmpArray);            
        }

        public bool GetXorValue()
        {
            bool result;

            result = states[0] ^ states[2] ^ states[4] ^ states[6];

            return result;
        }
    }
}
