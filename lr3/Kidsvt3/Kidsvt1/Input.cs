using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kidsvt1
{
    class Input : IComparable<Input>
    {

        public int CompareTo(Input input)
        {
            var inputObj = input;
            if (inputObj.InputValues == InputValues)
                return 0;
            else
                return 1;
        }

        public Input(KeyValuePair<BitArray, Dictionary<int, bool>> input)
        {
            InputValues = new BitArray(input.Key);
            CoveredFaults = new List<KeyValuePair<int, bool>>();
            foreach (var i in input.Value)
            {
                CoveredFaults.Add(i);
            }
        }

        public Input() { }

        public BitArray InputValues { get; set; }

        public List<KeyValuePair<int, bool>> CoveredFaults { get; set; }
    }
}
