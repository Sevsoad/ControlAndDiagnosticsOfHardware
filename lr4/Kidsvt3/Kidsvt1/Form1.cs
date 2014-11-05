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
        private List<Polynom> polynoms;
        private List<LogicElement> startElements;
        public Form1()
        {
            InitializeComponent();
            elements = new List<LogicElement>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitializeLogicElements();
            InitializePolynoms();

            var testSequence = GetTestInput();
            var result = TestPolynomsInSA(testSequence);

            textBox1.Text += result;
        }

        private List<List<bool>> GetTestInput()
        {
            var states = new BitArray(7);
            var combinationsCount = 0;
            var minInitList = new List<bool>();
            var inputsList = new List<bool>();
            var testList = new List<List<bool>>();
            var checker = new CoverageCheker(elements);

            IterateBits(states);
            for (var j = 1; j < 127; j++)
            {
                combinationsCount = 0;
                var initList = new List<bool>();
                foreach (var index in states)
                {
                    initList.Add((bool)index);
                }

                if (j == 92)
                {
                    LFSR lfsr1 = new LFSR(new List<bool>(initList));
                    var tempInputList = new List<List<bool>>();

                    while (checker.CheckCoversForInputs(tempInputList) < 26)
                    {
                        var tmpRandInput = new List<bool>();
                        lfsr1.Iterate();
                        tempInputList.Add(new List<bool>(lfsr1.states));

                        combinationsCount++;
                    }
                    testList = tempInputList;
                }
                
                IterateBits(states);
            }

            return testList;
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

        private void InitializePolynoms()
        {
            polynoms = new List<Polynom>();
            var polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[5] ^ x[4] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[4] ^ x[2] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 5,
                LogicOperation = (x, y) => (x[7] ^ x[6] ^ x[5] 
                    ^ x[4] ^ x[3] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[5] ^ x[4] ^ x[3] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[4] ^ x[2] ^ x[1] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[5] ^ x[2] ^ x[1] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 5,
                LogicOperation = (x, y) => (x[7] ^ x[6] ^ x[5]
                    ^ x[2] ^ x[1] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[6] ^ x[5] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[4] ^ x[2] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[6] ^ x[5] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 5,
                LogicOperation = (x, y) => (x[7] ^ x[6] ^ x[5]
                    ^ x[4] ^ x[3] ^ x[1] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[5] ^ x[4] ^ x[3] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[4] ^ x[2] ^ x[1] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[5] ^ x[2] ^ x[1] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 5,
                LogicOperation = (x, y) => (x[7] ^ x[6] ^ x[5]
                    ^ x[2] ^ x[1] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 3,
                LogicOperation = (x, y) => (x[7] ^ x[6] ^ x[5] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 1,
                LogicOperation = (x, y) => (x[2] ^ x[1] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 1,
                LogicOperation = (x, y) => (x[2] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 1,
                LogicOperation = (x, y) => (x[3] ^ x[2] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 1,
                LogicOperation = (x, y) => (x[3] ^ x[0] ^ y)
            };
            polynoms.Add(polynom);

            polynom = new Polynom
            {
                XorsCount = 1,
                LogicOperation = (x, y) => (x[4] ^ x[2] ^ y)
            };
            polynoms.Add(polynom);
        }

        private string TestPolynomsInSA(List<List<bool>> testSequences)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < polynoms.Count; i++)
            {
                var sa = new SA(polynoms[i], elements);
                sa.GenerateSignature(testSequences);
                var coveredFaults = sa.RunTestForAllFaults(testSequences);

                var coverage = String.Format("{0:0.00}", coveredFaults / 26.0f * 100);
                sb.Append("Polynom " + (i + 1) +": coverage "
                    + coverage
                    + "%, xor count : "
                    + polynoms[i].XorsCount + Environment.NewLine);
            }

            return sb.ToString();
        }       
    }
}
