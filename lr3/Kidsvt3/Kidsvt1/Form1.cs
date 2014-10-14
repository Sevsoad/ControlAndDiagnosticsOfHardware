using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kidsvt1
{
    public partial class Form1 : Form
    {
        const int TotalInputsCount = 13;
        private List<LogicElement> elements;
        private List<LogicElement> startElements;
        public Form1()
        {
            InitializeComponent();
            elements = new List<LogicElement>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitializeLogicElements();

            var allTests = new Dictionary<BitArray, Dictionary<int, bool>>();

            CalcCoversForAllInputs(allTests);
            label8.Text += "128 combinations of input values calculated \n";
            
            var minTests = CalcMinimumOfTest(allTests);
            label8.Text += "Minimal test coverege calculated: \n";
            StringBuilder sb = new StringBuilder();
            foreach (var test in minTests)
            {
                foreach(var value in test.InputValues){
                    sb.Append((bool)value ? "1" : "0");
                }
                sb.Append("  ");
            }
            sb.Append("\nTotal minimized test has: " + minTests.Count.ToString() + " inputs");    
            label8.Text += sb.ToString();

            label8.Text += "\nTotal switchings count: " + CalculateTotalSwitching(minTests);
            var positionedList = new List<Input>();
            label8.Text += "\nMinimized switchings count: " + MimimizeTotalSwitching(minTests, positionedList);
            label8.Text += "\nReordered test inputs:\n";
            StringBuilder sb2 = new StringBuilder();
            foreach (var test in positionedList)
            {
                foreach (var value in test.InputValues)
                {
                    sb2.Append((bool)value ? "1" : "0");
                }
                sb2.Append("  ");
            }
            label8.Text += sb2.ToString();
            label8.Text += "\n";
        }

        private int MimimizeTotalSwitching(List<Input> inputsList, List<Input> resultList)
        {
            var tempInputsList = new List<Input>();
            var minimumSwitches = 20;
            for (var i = 0; i < inputsList.Count; i++)
            {
                var k = 0;
                for (var j = 0; j < inputsList.Count; j++)
                {
                    if (i + j < inputsList.Count)
                    {
                        tempInputsList.Add(inputsList[j + i]);
                    }
                    else
                    {
                        tempInputsList.Add(inputsList[k]);
                        k++;
                    }
                }

                for (var h = 0; h < inputsList.Count - 1; h++ )
                {
                    var temp = tempInputsList[h];
                    tempInputsList[h] = tempInputsList[h + 1];
                    tempInputsList[h + 1] = temp;

                    var tempResult = CalculateTotalSwitching(tempInputsList);
                    if (tempResult < minimumSwitches)
                    {
                        minimumSwitches = tempResult;
                        resultList.Clear();
                        resultList.AddRange(tempInputsList);
                    }
                }
                
                tempInputsList.Clear();
            }
            return minimumSwitches;
        }

        private int CalculateTotalSwitching(List<Input> inputsList)
        {
            var totalSwitching = 0;
            for (var i = 0; i < inputsList.Count; i++)
            {
                if (i != inputsList.Count - 1)
                {
                    totalSwitching += CountSwitching(inputsList[i].InputValues,
                        inputsList[i + 1].InputValues);
                }
                else
                {
                    totalSwitching += CountSwitching(inputsList[i].InputValues,
                        inputsList[0].InputValues);
                }
            }

            return totalSwitching;
        }

        private int CountSwitching(BitArray initInput, BitArray changedInput)
        {
            var initResults = new List<bool>();
            var changedResults = new List<bool>();
            var totalSwitched = 0;

            var el1result = elements[0].LogicOperation(
                        new[] { initInput[0], initInput[1] });
            initResults.Add(el1result);

            var el2result = elements[1].LogicOperation(
                    new[] { initInput[2] });
            initResults.Add(el2result);

            var el3result = elements[2].LogicOperation(
                    new[] { initInput[4], initInput[5] });
            initResults.Add(el3result);

            var el4result = elements[3].LogicOperation(
                    new[] { initInput[3], initInput[6], el3result });
            initResults.Add(el4result);

            var el5result = elements[4].LogicOperation(
                    new[] { el2result, el4result });
            initResults.Add(el5result);

            var el6result = elements[5].LogicOperation(
                    new[] { el1result, el5result });
            initResults.Add(el6result);

            el1result = elements[0].LogicOperation(
                        new[] { changedInput[0], changedInput[1] });
            changedResults.Add(el1result);

            el2result = elements[1].LogicOperation(
                    new[] { changedInput[2] });
            changedResults.Add(el2result);

            el3result = elements[2].LogicOperation(
                    new[] { changedInput[4], changedInput[5] });
            changedResults.Add(el3result);

            el4result = elements[3].LogicOperation(
                    new[] { changedInput[3], changedInput[6], el3result });
            changedResults.Add(el4result);

            el5result = elements[4].LogicOperation(
                    new[] { el2result, el4result });
            changedResults.Add(el5result);

            el6result = elements[5].LogicOperation(
                    new[] { el1result, el5result });
            changedResults.Add(el6result);

            for (var i = 0; i < changedResults.Count; i++)
            {
                if (changedResults[i] != initResults[i])
                {
                    totalSwitched++;
                }
            }

            return totalSwitched;
        }

        private List<Input> CalcMinimumOfTest(
            Dictionary<BitArray, Dictionary<int, bool>> allInputs)
        {            
            var maxCoveredInt = 0;
            var testMinimum = new List<Input>();
            Input maxCovered = null;

            foreach (var input in allInputs)
            {
                if (input.Value.Count > maxCoveredInt)
                {
                    maxCovered = new Input(input);
                    maxCoveredInt = maxCovered.CoveredFaults.Count;
                }
            }

            if (maxCovered != null)
            {
                testMinimum.Add(maxCovered);
                while (CountUniqueValuesInInputList(testMinimum) < 26)
                {
                    var maxUncoveredInput = GetInputWithMaxUncovered(allInputs, testMinimum);
                    testMinimum.Add(maxUncoveredInput);
                }
            }

            return testMinimum;
        }

        private Input GetInputWithMaxUncovered(
            Dictionary<BitArray, Dictionary<int, bool>> inputs, List<Input> currentInputlist)
        {
            KeyValuePair<Input, int> tempInput = new KeyValuePair<Input,int>(null, 0);

            foreach (var input in inputs)
            {
                var uncoveredFaults = 0;
                foreach (var pair2 in input.Value)
                {
                    var contains = false;
                    foreach (var listInput in currentInputlist)
                    {
                        
                        foreach (var pair1 in listInput.CoveredFaults)
                        {
                            if (pair1.Key == pair2.Key && pair1.Value == pair2.Value)
                            {
                                contains = true;
                                break;
                            }  
                        }
                        if (contains)
                        {
                            break;
                        }
                    }
                    if (!contains)
                    {
                        uncoveredFaults++;
                    }
                }
                if (uncoveredFaults > tempInput.Value)
                {
                    tempInput = new KeyValuePair<Input, int>(
                        new Input(input), uncoveredFaults);
                }                
            }
            return tempInput.Key;
        }

        private bool CheckIfListHasDictVals(List<Input> minList, Dictionary<int, bool> dict2)
        {
            var result = true;
            bool contains;
            bool uncovered;
            for (var i = 0; i < minList.Count; i++)
            {
                var dict1 = minList[i].CoveredFaults;
                uncovered = false;
                foreach (var pairIn2 in dict2)
                {
                    contains = false;
                    foreach (var pairIn1 in dict1)
                    {
                        if (pairIn1.Key == pairIn2.Key && pairIn1.Value == pairIn2.Value)
                        {
                            contains = true;
                            break;
                        }                        
                    }
                    if (!contains)
                    {
                        result = false;
                        uncovered = true;
                        break;
                    }
                }

                if (!uncovered)
                {
                    result = true;
                    break;
                }
            }
            
            return result;
        }

        private int CountUniqueValuesInInputList(List<Input> inputList)
        {
            var inputWithUniqueValuse = new Input();
            inputWithUniqueValuse.CoveredFaults = 
                new List<KeyValuePair<int, bool>>(inputList[0].CoveredFaults);
            var dict2 = inputWithUniqueValuse.CoveredFaults;
            var result = 0;

            for (var i = 1; i < inputList.Count; i++)
            {
                var dict1 = inputList[i].CoveredFaults;
                foreach (var pairIn1 in dict1)
                {
                    bool contains = false;
                    foreach (var pairIn2 in dict2)
                    {
                        if (pairIn1.Key == pairIn2.Key && pairIn1.Value == pairIn2.Value)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                    {
                        dict2.Add(pairIn1);
                    }
                }
            }
            result = dict2.Count;
            return result;
        }

        private void CalcCoversForAllInputs(Dictionary<BitArray, Dictionary<int, bool>> result)
        {
            BitArray combinations = new BitArray(7, false);
            List<bool> outPole = new List<bool>();
            var outPoleTmp = new List<bool>();
            var covers = new Dictionary<int, bool>();

            for (var i = 0; i < 128; i++)
            {                
                for (var k = 0; k < combinations.Count; k++ )
                {
                    outPole.Add(combinations[k]);
                }

                var schemeResult = false;
                var el1result = elements[0].LogicOperation(
                        new[] { combinations[0], combinations[1] });
                outPole.Add(el1result);

                var el2result = elements[1].LogicOperation(
                        new[] { combinations[2]});
                outPole.Add(el2result);

                var el3result = elements[2].LogicOperation(
                        new[] { combinations[4], combinations[5] });
                outPole.Add(el3result);

                var el4result = elements[3].LogicOperation(
                        new[] { combinations[3], combinations[6], el3result });
                outPole.Add(el4result);

                var el5result = elements[4].LogicOperation(
                        new[] { el2result, el4result });
                outPole.Add(el5result);

                var el6result = elements[5].LogicOperation(
                        new[] { el1result, el5result });
                outPole.Add(el6result);

                schemeResult = el6result;                
                for (var j = 0; j < 13; j++)
                {
                    var el1resultTmp = el1result;
                    var el2resultTmp = el2result;
                    var el3resultTmp = el3result;
                    var el4resultTmp = el4result;
                    var el5resultTmp = el5result;
                    var el6resultTmp = el6result;
                    outPoleTmp.AddRange(outPole);
                    if (j < 7)
                    {
                        outPoleTmp[j] = outPoleTmp[j] ? false : true;
                    }
                    el1resultTmp = elements[0].LogicOperation(
                        new[] { outPoleTmp[0], outPoleTmp[1] });
                    if (j == 7)
                    {
                        el1resultTmp = el1resultTmp ? false : true;
                    }
                    el2resultTmp = elements[1].LogicOperation(
                        new[] { outPoleTmp[2] });
                    if (j == 8)
                    {
                        el2resultTmp = el2resultTmp ? false : true;
                    }
                    el3resultTmp = elements[2].LogicOperation(
                        new[] { outPoleTmp[4], outPoleTmp[5] });
                    if (j == 9)
                    {
                        el3resultTmp = el3resultTmp ? false : true;
                    }
                    el4resultTmp = elements[3].LogicOperation(
                        new[] { outPoleTmp[3], outPoleTmp[6], el3resultTmp });
                    if (j == 10)
                    {
                        el4resultTmp = el4resultTmp ? false : true;
                    }
                    el5resultTmp = elements[4].LogicOperation(
                            new[] { el2resultTmp, el4resultTmp });
                    if (j == 11)
                    {
                        el5resultTmp = el5resultTmp ? false : true;
                    }
                    el6resultTmp = elements[5].LogicOperation(
                            new[] { el1resultTmp, el5resultTmp });
                    if (j == 12)
                    {
                        el6resultTmp = el6resultTmp ? false : true;
                    }
                    if (el6resultTmp != schemeResult)
                    {
                        covers.Add(j, outPole[j] ? false : true);
                    }
                    outPoleTmp.Clear();
                }
                outPole.Clear();                
                var tmpDict = new Dictionary<int, bool>(covers);
                result.Add((BitArray)combinations.Clone(), tmpDict);
                covers.Clear();
                ClearCoveredFaults();
                IterateBits(combinations);                
            }
            
        }

        private int ExecuteScheme(Dictionary<int, int> inputSequence, int faultElemId, bool faultVal)
        {
            
            var el1result = elements[0].LogicOperation(
                new[]{inputSequence[1] == 1 ? true : false, inputSequence[2] == 1 ? true : false});
            if (elements[0].Number == faultElemId)
            {
                el1result = faultVal;
            }

            var el2result = elements[1].LogicOperation(
                new[] { inputSequence[3] == 1 ? true : false});
            if (elements[1].Number == faultElemId)
            {
                el2result = faultVal;
            }

            var el3result = elements[2].LogicOperation(
                new[] { inputSequence[5] == 1 ? true : false, inputSequence[6] == 1 ? true : false });
            if (elements[2].Number == faultElemId)
            {
                el3result = faultVal;
            }

            var el4result = elements[3].LogicOperation(
                new[] { inputSequence[4] == 1 ? true : false, inputSequence[7] == 1 ? true : false, el3result });
            if (elements[3].Number == faultElemId)
            {
                el4result = faultVal;
            }

            var el5result = elements[4].LogicOperation(
               new[] { el2result, el4result });
            if (elements[4].Number == faultElemId)
            {
                el5result = faultVal;
            }
            var el6result = elements[5].LogicOperation(
               new[] { el1result, el5result });
            if (elements[5].Number == faultElemId)
            {
                el6result = faultVal;
            }

            return el6result == true ? 1 : 0;
        }

        private void ClearCoveredFaults()
        {
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i].Const0Covered = false;
                elements[i].Const1Covered = false;
            }
        }

        private void IterateBits(BitArray combinations)
        {
            bool carry = true;
            for (var j = 0; j < 7; j++)
            {
                if (carry == true)
                {
                    if (combinations[j] == false)
                    {
                        combinations[j] = true;
                        carry = false;
                    }
                    else
                    {
                        combinations[j] = false;
                        carry = true;
                    }
                }
            }
        }
        
        private void InitializeLogicElements()
        {            
            var element = new LogicElement
            {
                LogicOperation = (x) => !(x[0] || x[1]),
                Number = 1,
                InputNumbers = new[] { 1, 2 },
                IsStart = true
            };
            elements.Add(element);

            element = new LogicElement
            {
                LogicOperation = (x) => !(x[0]),
                Number = 2,
                InputNumbers = new[] { 3 },
                IsStart = true
            };
            elements.Add(element);

            element = new LogicElement
            {
                LogicOperation = (x) => !(x[0] && x[1]),
                Number = 3,
                InputNumbers = new[] { 5, 6 },
                IsStart = true
            };
            elements.Add(element);

            element = new LogicElement
            {
                LogicOperation = (x) => (x[0] && x[1] && x[2]),
                Number = 4,
                InputNumbers = new[] { 4, 7 },
                IsStart = true
            };
            elements.Add(element);

            element = new LogicElement
            {
                LogicOperation = (x) => (x[0] || x[1]),
                Number = 5
            };
            elements.Add(element);

            element = new LogicElement
            {
                LogicOperation = (x) => (x[0] && x[1]),
                Number = 6,
                IsResult = true
            };
            elements.Add(element);

            elements[0].Right = elements[5];
            elements[1].Right = elements[4];
            elements[2].Right = elements[3];

            elements[3].Left = new[]{elements[2]};
            elements[3].Right = elements[4];

            elements[4].Left = new[]{elements[1], elements[3]};
            elements[4].Right = elements[5];
            elements[5].Left = new[]{elements[0], elements[4]};

            //inputs
            element = new LogicElement
            {
                Number = 1,
                IsInputWire = true,
                Right = elements[0]
            };
            elements.Add(element);

            element = new LogicElement
            {
                Number = 2,
                IsInputWire = true,
                Right = elements[0]
            };
            elements.Add(element);

            element = new LogicElement
            {
                Number = 3,
                IsInputWire = true,
                Right = elements[1]
            };
            elements.Add(element);

            element = new LogicElement
            {
                Number = 4,
                IsInputWire = true,
                Right = elements[3]
            };
            elements.Add(element);

            element = new LogicElement
            {
                Number = 5,
                IsInputWire = true,
                Right = elements[2]
            };
            elements.Add(element);

            element = new LogicElement
            {
                Number = 6,
                IsInputWire = true,
                Right = elements[2]
            };
            elements.Add(element);

            element = new LogicElement
            {
                Number = 7,
                IsInputWire = true,
                Right = elements[3]
            };
            elements.Add(element);

            startElements = new List<LogicElement>();
            foreach (var el in elements)
            {
                if (el.IsInputWire)
                {
                    startElements.Add(el);
                }
            }
        }
       
    }
}
