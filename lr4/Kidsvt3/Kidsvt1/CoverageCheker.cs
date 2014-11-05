using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kidsvt1
{
    class CoverageCheker
    {
        List<LogicElement> elements;
        List<bool> polesResults;

        public CoverageCheker(List<LogicElement> inElements)
        {
            elements = inElements;
            polesResults = new List<bool>();
        }

        public int CheckCoversForInputs(List<List<bool>> coversList)
        {
            var coveredFaults = 0;
            for (var i = 0; i < coversList.Count; i++)
            {
                polesResults = new List<bool>();
                InicializeEthalon(coversList[i]);

                Check0Covers();
                Check1Covers();

                coveredFaults = CalculateCoveredFaults(elements);                
            }

            ClearCoveredFaults(elements);
            return coveredFaults;
        }        

        private void Check0Covers()
        {
            for (var j = 0; j < polesResults.Count; j++)
            {
                var outPolesCopy = new List<bool>(polesResults);

                if (!elements[j].Const0Covered && j < 7)
                {
                    if (outPolesCopy[j])
                        outPolesCopy[j] = false;
                    else
                        continue;
                }

                var el1resultTest = elements[0].LogicOperation(
                   new[] { outPolesCopy[0], outPolesCopy[1] });
                if (j == 7)
                {
                    if (!el1resultTest)
                        continue;
                    else
                        el1resultTest = false;
                }

                var el2resultTest = elements[1].LogicOperation(
                        new[] { outPolesCopy[2] });
                if (j == 8)
                {
                    if (!el2resultTest)
                        continue;
                    else
                        el2resultTest = false;
                }

                var el3resultTest = elements[2].LogicOperation(
                        new[] { outPolesCopy[4], outPolesCopy[5] });
                if (j == 9)
                {
                    if (!el3resultTest)
                        continue;
                    else
                        el3resultTest = false;
                }

                var el4resultTest = elements[3].LogicOperation(
                        new[] { outPolesCopy[3], outPolesCopy[6], el3resultTest });
                if (j == 10)
                {
                    if (!el4resultTest)
                        continue;
                    else
                        el4resultTest = false;
                }

                var el5resultTest = elements[4].LogicOperation(
                        new[] { el2resultTest, el4resultTest });
                if (j == 11)
                {
                    if (!el5resultTest)
                        continue;
                    else
                        el5resultTest = false;
                }

                var el6resultTest = elements[5].LogicOperation(
                        new[] { el1resultTest, el5resultTest });
                if (j == 12)
                {
                    if (!el6resultTest)
                        continue;
                    else
                        el6resultTest = false;
                }

                if (el6resultTest != polesResults[polesResults.Count - 1])
                {
                    if (!elements[j].Const0Covered)
                    {
                        elements[j].Const0Covered = true;
                    }
                }
            }
        }

        private void Check1Covers()
        {
            for (var j = 0; j < polesResults.Count; j++)
            {
                var outPolesCopy = new List<bool>(polesResults);

                if (!elements[j].Const1Covered && j < 7)
                {
                    if (!outPolesCopy[j])
                        outPolesCopy[j] = true;
                    else
                        continue;
                }

                var el1resultTest = elements[0].LogicOperation(
                   new[] { outPolesCopy[0], outPolesCopy[1] });
                if (j == 7)
                {
                    if (el1resultTest)
                        continue;
                    else
                        el1resultTest = true;
                }

                var el2resultTest = elements[1].LogicOperation(
                        new[] { outPolesCopy[2] });
                if (j == 8)
                {
                    if (el2resultTest)
                        continue;
                    else
                        el2resultTest = true;
                }

                var el3resultTest = elements[2].LogicOperation(
                        new[] { outPolesCopy[4], outPolesCopy[5] });
                if (j == 9)
                {
                    if (el3resultTest)
                        continue;
                    else
                        el3resultTest = true;
                }

                var el4resultTest = elements[3].LogicOperation(
                        new[] { outPolesCopy[3], outPolesCopy[6], el3resultTest });
                if (j == 10)
                {
                    if (el4resultTest)
                        continue;
                    else
                        el4resultTest = true;
                }

                var el5resultTest = elements[4].LogicOperation(
                        new[] { el2resultTest, el4resultTest });
                if (j == 11)
                {
                    if (el5resultTest)
                        continue;
                    else
                        el5resultTest = true;
                }

                var el6resultTest = elements[5].LogicOperation(
                        new[] { el1resultTest, el5resultTest });
                if (j == 12)
                {
                    if (el6resultTest)
                        continue;
                    else
                        el6resultTest = true;
                }

                if (el6resultTest != polesResults[polesResults.Count - 1])
                {
                    if (!elements[j].Const1Covered)
                    {
                        elements[j].Const1Covered = true;
                    }
                }
            }
        }

        private void InicializeEthalon(List<bool> combination)
        {
            for (var k = 0; k < combination.Count; k++)
            {
                polesResults.Add(combination[k]);
            }

            var el1result = elements[0].LogicOperation(
                    new[] { combination[0], combination[1] });
            polesResults.Add(el1result);

            var el2result = elements[1].LogicOperation(
                    new[] { combination[2] });
            polesResults.Add(el2result);

            var el3result = elements[2].LogicOperation(
                    new[] { combination[4], combination[5] });
            polesResults.Add(el3result);

            var el4result = elements[3].LogicOperation(
                    new[] { combination[3], combination[6], el3result });
            polesResults.Add(el4result);

            var el5result = elements[4].LogicOperation(
                    new[] { el2result, el4result });
            polesResults.Add(el5result);

            var el6result = elements[5].LogicOperation(
                    new[] { el1result, el5result });
            polesResults.Add(el6result);
        }

        public static int CalculateCoveredFaults(List<LogicElement> elements)
        {
            var coveredFaults = 0;
            foreach (var item in elements)
            {
                if (item.Const0Covered)
                {
                    coveredFaults++;
                }
                if (item.Const1Covered)
                {
                    coveredFaults++;
                }
            }

            return coveredFaults;
        }

        public static void ClearCoveredFaults(List<LogicElement> elements)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i].Const0Covered = false;
                elements[i].Const1Covered = false;
            }
        }
    }
}
