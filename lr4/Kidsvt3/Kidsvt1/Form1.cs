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

            textBox1.Text += "LFSR random generation started" + Environment.NewLine;
            var minInitList = 0;
            var minInitState = GetMinInitState(out minInitList);
            
            textBox1.Text += "Calculated rational initial state:";

            foreach (var i in minInitState)
            {
                textBox1.Text += i ? "1 " : "0 ";
            }

            textBox1.Text += Environment.NewLine + "Number of test inputs to cover: " + minInitList;
        }

        private List<bool> GetMinInitState(out int iterationsToCover)
        {
            var states = new BitArray(7);
            var combinationsCount = 0;
            var minInitList = new List<bool>();
            var minCombCount = 200;
            var iterations = 0;

            IterateBits(states);
            for (var j = 1; j < 128; j++)
            {
                combinationsCount = 0;
                var initList = new List<bool>();
                foreach (var index in states)
                {
                    initList.Add((bool)index);
                }

                LFSR lfsr1 = new LFSR(initList);
                var tempInputList = new List<List<bool>>();

                while (CalculateCoveredFaults(tempInputList) < 26)
                {
                    var tmpRandInput = new List<bool>();
                    for (var i = 0; i < 7; i++)
                    {
                        tmpRandInput.Add(lfsr1.states[6]);
                        lfsr1.Iterate();
                    }
                    tempInputList.Add(new List<bool>(lfsr1.states));

                    combinationsCount++;
                }

                if (combinationsCount < minCombCount)
                {
                    minCombCount = combinationsCount;
                    minInitList.RemoveRange(0, minInitList.Count);
                    minInitList.AddRange(initList);
                }
                iterations++;
                IterateBits(states);
            }

            iterationsToCover = minCombCount;
            return minInitList;
        }

        private int CalculateCoveredFaults(List<List<bool>> coversList)
        {
            var outPole = new List<bool>();
            var outPoleTmp = new List<bool>();
            var coveredFaults = 0;

            for (var i = 0; i < coversList.Count; i++)
            {
                var combinations = coversList[i];

                for (var k = 0; k < combinations.Count; k++)
                {
                    outPole.Add(combinations[k]);
                }

                var el1result = elements[0].LogicOperation(
                        new[] { combinations[0], combinations[1] });
                outPole.Add(el1result);

                var el2result = elements[1].LogicOperation(
                        new[] { combinations[2] });
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

                for (var j = 0; j < 6; j++)
                {
                    if (!elements[j].Const0Covered)
                        elements[j].Const0Covered = outPole[j + combinations.Count];

                    if (!elements[j].Const1Covered)
                        elements[j].Const1Covered = !outPole[j + combinations.Count];
                }

                for (var j = 0; j < combinations.Count; j++)
                {
                    if (!elements[j + 6].Const0Covered)
                        elements[j + 6].Const0Covered = outPole[j];

                    if (!elements[j + 6].Const1Covered)
                    elements[j + 6].Const1Covered = !outPole[j];
                }

                outPole = new List<bool>();
            }

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

            ClearCoveredFaults();

            return coveredFaults;
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
