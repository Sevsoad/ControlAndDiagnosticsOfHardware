using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kidsvt1
{
    class SA
    {
        List<bool> signature;
        public List<bool> state;
        public List<LogicElement> elements;
        Polynom polynom;

        public SA(Polynom polynom, List<LogicElement> elements)
        {
            state = new List<bool>();
            this.elements = elements;
            this.polynom = polynom;
            for (var i = 0; i < 8; i++)
            {
                state.Add(false);
            }
        }

        private void Iterate(bool inValue)
        {
            bool tmp = state[0];
            var tmpArray = new List<bool>(state);

            tmpArray.RemoveAt(tmpArray.Count - 1);

            state[0] = polynom.LogicOperation(state, inValue);
            state.RemoveRange(1, 7);
            state.AddRange(tmpArray);
        }

        public void GenerateSignature(List<List<bool>> testSequences)
        {
            foreach (var sequence in testSequences)
            {
                var schemeResult = ExecuteScheme(sequence);
                Iterate(schemeResult);
            }

            signature = new List<bool>(state);
        }

        public int RunTestForAllFaults(List<List<bool>> testSequences) 
        {
            
            for (var i = 0; i < 13; i++)
            {
                ResetSAState();

                foreach (var sequence in testSequences)
                {
                    var schemeResult = ExecuteSchemeWithFault(sequence, i, false);
                    Iterate(schemeResult);
                }

                if (!CompareStateToSignature())
                {
                    elements[i].Const0Covered = true;
                }
            }

            for (var i = 0; i < 13; i++)
            {
                ResetSAState();

                foreach (var sequence in testSequences)
                {
                    var schemeResult = ExecuteSchemeWithFault(sequence, i, true);
                    Iterate(schemeResult);
                }

                if (!CompareStateToSignature())
                {
                    elements[i].Const1Covered = true;
                }
            }

            var faultsNumber = CoverageCheker.CalculateCoveredFaults(elements);
            CoverageCheker.ClearCoveredFaults(elements);

            return faultsNumber;
        } 

        private bool ExecuteScheme(List<bool> combination)
        {
            var el1result = elements[0].LogicOperation(
                    new[] { combination[0], combination[1] });

            var el2result = elements[1].LogicOperation(
                    new[] { combination[2] });

            var el3result = elements[2].LogicOperation(
                    new[] { combination[4], combination[5] });

            var el4result = elements[3].LogicOperation(
                    new[] { combination[3], combination[6], el3result });

            var el5result = elements[4].LogicOperation(
                    new[] { el2result, el4result });

            var el6result = elements[5].LogicOperation(
                    new[] { el1result, el5result });

            return el6result;
        }

        private bool ExecuteSchemeWithFault(List<bool> inCombination, int numberOfpole,
            bool faultType)
        {
            var combination = new List<bool>(inCombination);

            if (numberOfpole < 7)
            {
                combination[numberOfpole] = faultType;
            }

            var el1result = numberOfpole == 7 ? faultType : elements[0].LogicOperation(
                    new[] { combination[0], combination[1] });


            var el2result = numberOfpole == 8 ? faultType : elements[1].LogicOperation(
                    new[] { combination[2] });

            var el3result = numberOfpole == 9 ? faultType : elements[2].LogicOperation(
                    new[] { combination[4], combination[5] });

            var el4result = numberOfpole == 10 ? faultType : elements[3].LogicOperation(
                    new[] { combination[3], combination[6], el3result });

            var el5result = numberOfpole == 11 ? faultType : elements[4].LogicOperation(
                    new[] { el2result, el4result });

            var el6result = numberOfpole == 12 ? faultType : elements[5].LogicOperation(
                    new[] { el1result, el5result });

            return el6result;
        }

        private void ResetSAState()
        {
            for (var i = 0; i < state.Count; i++)
            {
                state[i] = false;
            }
        }

        private bool CompareStateToSignature()
        {
            for (var i = 0; i < signature.Count; i++)
            {
                if (signature[i] != state[i])
                    return false;
            }
            return true;
        }

    }
}
