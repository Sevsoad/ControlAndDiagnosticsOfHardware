using System;
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
        public Form1()
        {
            InitializeComponent();
            elements = new List<LogicElement>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!CheckInput())
            {
                MessageBox.Show("Please, set one stack-at fault.");
                return;
            }
            InitializeLogicElements();

            var faultElemId = GetCheckedControlNumber();

            label8.Text = "Node number " + (faultElemId).ToString() + "\n";
            var currentElement = elements[faultElemId - 1];
            var faultCube = GetDFaultCube(currentElement, GetFaultType());

            label8.Text += "Primitive D-fault cube: \n";
            for (var i = 0; i < faultCube.ColumnsNumbers.Length; i++)
            {
                label8.Text += faultCube.ColumnsNumbers[i] + "-" + faultCube.ColumnValues[i] + " ";
            }
            label8.Text += "\n";

            var dPassCube = CalcDPass(currentElement, faultCube);
            if (dPassCube == null)
            {
                label8.Text += "Cannot calculate cube for D-pass. \n";
                return;
            }
            label8.Text += "D-cube after d-pass: \n";
            for (var i = 0; i < dPassCube.ColumnsNumbers.Length; i++)
            {
                label8.Text += dPassCube.ColumnsNumbers[i] + "-" + dPassCube.ColumnValues[i] + " ";
            } 
            label8.Text += "\n";

            var result = CrossWithOtherSingles(dPassCube, currentElement);

            label8.Text += "Result: \n";
            for (var i = 0; i < TotalInputsCount; i++)
            {
                label8.Text += result.ColumnsNumbers[i] + "-" + result.ColumnValues[i] + " ";
            }
            label8.Text += "\n";
        }

        private Cube CrossWithOtherSingles(Cube currentCube, LogicElement faultElement)
        {
            if (currentCube.ColumnsNumbers.Length != TotalInputsCount)
            {
                var tmpCube = new Cube();
                tmpCube.ColumnsNumbers = new int[TotalInputsCount];
                tmpCube.ColumnValues = new char[TotalInputsCount];

                for (var i = 0; i < TotalInputsCount; i++)
                {
                    tmpCube.ColumnsNumbers[i] = i + 1;
                    tmpCube.ColumnValues[i] = 'x';
                }

                for (var i = 0; i < currentCube.ColumnsNumbers.Length; i++)
                {
                    tmpCube.ColumnValues[currentCube.ColumnsNumbers[i] - 1] = currentCube.ColumnValues[i];
                }

                currentCube = tmpCube;
            }

            var alreadyCountedElements = new List<LogicElement>();
            var tmpElement = faultElement;
            while (tmpElement != null)
            {
                alreadyCountedElements.Add(tmpElement);
                tmpElement = tmpElement.Right;
            }

            for(var i = elements.Count - 1; i >= 0 ; i--)
            {
                var element = elements[i];
                if (!alreadyCountedElements.Contains(element))
                {
                    Cube elemSingCube = null;
                    var elemResultOut = element.SingularCubes[0].ColumnsNumbers.Last();
                    var expectedCubeOut = currentCube.ColumnValues[elemResultOut - 1];
                    if (expectedCubeOut == 'x')
                    {
                        elemSingCube = element.SingularCubes[0];
                    }
                    else
                    {
                        var outOfaCube = '\0';
                        if (expectedCubeOut == '1' || expectedCubeOut == 'd')
                        {
                            outOfaCube = '1';
                        }
                        else {
                            outOfaCube = '0';
                        }
                        foreach (var cube in element.SingularCubes){
                            if (cube.ColumnValues.Last() == outOfaCube)
                            {
                                elemSingCube = cube;
                            }
                        }
                    }

                    currentCube = CrossDAndSingleCube(currentCube, elemSingCube);
                }
            }

            return currentCube;
        }

        private Cube CalcDPass(LogicElement elem, Cube faultCube)
        {
            if (elem.IsResult)
            {
                return faultCube;
            }
            var tmp = faultCube;
            var curElem = elem.Right;

            while (curElem != null)
            {
                Cube concatCube = null;

                foreach(var cube in curElem.DCubes)
                {
                    concatCube = ConcatDCubes(tmp, cube);
                    if (concatCube == null)
                    {
                        if(curElem.DCubes.Last() == cube)
                        {
                            return null;
                        }
                        continue;
                    }
                    break;
                }
                tmp = concatCube;
                curElem = curElem.Right;
            }

            return tmp;
        }

        private Cube GetDFaultCube(LogicElement elem, int faultType)
        {
            var character = faultType == 0 ? 'd' : 'n';

            foreach (var cube in elem.DCubes)
            {
                if (cube.ColumnValues.Last() == character)
                {
                    return cube;
                }
            }

            return null;
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
        
        private bool CheckInput()
        {
            var faults = 0;
            foreach (Control c in this.Controls)
            {
                if (c is GroupBox)
                {
                    if (c.Controls.Count != 0)
                    {
                        if (c.Controls[0].Text == "const 1" || c.Controls[0].Text == "const 0")
                        {
                            faults++;
                        }
                    }
                }
            }
            if (faults != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private int GetCheckedControlNumber()
        {
            foreach (Control c in this.Controls)
            {
                if (c is GroupBox)
                {
                    if (c.Controls.Count != 0)
                    {
                        if (c.Controls[0].Text == "const 1" || c.Controls[0].Text == "const 0")
                        {
                            return int.Parse(Regex.Replace(c.Name, "[^0-9]+", string.Empty));
                        }
                    }
                }
            }

            return -1;
        }

        private int GetFaultType()
        {
            foreach (Control c in this.Controls)
            {
                if (c is GroupBox)
                {
                    if (c.Controls.Count != 0)
                    {
                        if (c.Controls[0].Text == "const 1" || c.Controls[0].Text == "const 0")
                        {
                            return int.Parse(Regex.Replace(c.Controls[0].Text, "[^0-9]+", string.Empty));
                        }
                    }
                }
            }

            return -1;
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

            foreach (var elem in elements)
            {
                InitSinglCubes(elem);
                CalcDCubes(elem);
            }
        }

        private void InitSinglCubes(LogicElement element)
        {            
            ResourceManager rm = new ResourceManager("Kidsvt1.Input values", Assembly.GetExecutingAssembly());
            var singlCubeStr = rm.GetString("Element" + element.Number);

            string[] singularCubes = singlCubeStr.Split(';');

            var singlCubes = new List<Cube>();

            foreach (var cube in singularCubes)
            {
                var cubeValues = cube.Trim().Split(',');
                var tmpCube = new Cube {ColumnsNumbers = new int[cubeValues.Length],
                    ColumnValues = new char[cubeValues.Length]};

                for(var j = 0; j < cubeValues.Length; j++)
                {
                    var splittedValues = cubeValues[j].Split('-');
                    tmpCube.ColumnsNumbers[j] = int.Parse(splittedValues[0]);
                    tmpCube.ColumnValues[j] = char.Parse(splittedValues[1]);
                }

                singlCubes.Add(tmpCube);
            }

            element.SingularCubes = singlCubes;

            return;
        }

        private void CalcDCubes(LogicElement element)
        {
            var singlCubes = element.SingularCubes;
            var DCubes = new List<Cube>();

            for (var i = 0; i < singlCubes.Count; i++)
            {
                var cube = singlCubes[i];
                foreach (var tmpCube in singlCubes)
                {
                    if (cube != tmpCube)
                    {
                        var resultCube = CrossCubes(cube, tmpCube);
                        if (resultCube != null)
                        {
                            DCubes.Add(resultCube);
                        }                            
                    }
                }       
            }

            element.DCubes = DCubes;
        }

        private Cube CrossCubes(Cube c1, Cube c2)
        {
            var length = c1.ColumnValues.Length;
            if ((c1.ColumnValues[length - 1] == c2.ColumnValues[length - 1]) || c2.ColumnsNumbers.Length != length)
            {
                return null;
            }

            for (var i = 0; i < c1.ColumnsNumbers.Length; i++)
            {
                if (c1.ColumnsNumbers[i] != c2.ColumnsNumbers[i])
                {
                    return null;
                }
            }

            var result = new Cube();
            result.ColumnsNumbers = new int[c1.ColumnsNumbers.Length];
            result.ColumnValues = new char[c1.ColumnsNumbers.Length];
            Array.Copy(c1.ColumnsNumbers, result.ColumnsNumbers, c1.ColumnsNumbers.Length);

            var tmpValues = new char[c1.ColumnsNumbers.Length];

            for (var i = 0; i < c1.ColumnsNumbers.Length; i++)
            {
                var c1Val = c1.ColumnValues[i];
                var c2Val = c2.ColumnValues[i];

                if (c1Val == 'x' && c2Val == 'x')
                {
                    tmpValues[i] = 'x';
                    continue;
                }
                if (c1Val == '1' && c2Val == '0')
                {
                    tmpValues[i] = 'd';
                    continue;
                }
                if (c1Val == '0' && c2Val == '1')
                {
                    tmpValues[i] = 'n';
                    continue;
                }
                if ((c1Val == '1' && c2Val == '1') || (c1Val == '1' && c2Val == 'x') ||
                    (c1Val == 'x' && c2Val == '1'))
                {
                    tmpValues[i] = '1';
                    continue;
                }
                if ((c1Val == '0' && c2Val == '0') || (c1Val == '0' && c2Val == 'x') ||
                    (c1Val == 'x' && c2Val == '0'))
                {
                    tmpValues[i] = '0';
                }
            }

            Array.Copy(tmpValues, result.ColumnValues, tmpValues.Length);           

            return result;
        }

        private Cube ConcatDCubes(Cube c1, Cube c2)
        {
            Cube resultCube = new Cube();

            if (c1.ColumnsNumbers.Length != TotalInputsCount)
            {
                var tmpCube = new Cube();
                tmpCube.ColumnsNumbers = new int[TotalInputsCount];
                tmpCube.ColumnValues = new char[TotalInputsCount];

                for (var i = 0; i < TotalInputsCount; i++)
                {
                    tmpCube.ColumnsNumbers[i] = i + 1;
                    tmpCube.ColumnValues[i] = 'x';
                }               

                for(var i = 0; i < c1.ColumnsNumbers.Length; i++)
                {
                    tmpCube.ColumnValues[c1.ColumnsNumbers[i] - 1] = c1.ColumnValues[i];
                }

                resultCube = tmpCube;
            }
            else {
                resultCube.ColumnsNumbers = new int[TotalInputsCount];
                resultCube.ColumnValues = new char[TotalInputsCount];
                Array.Copy(c1.ColumnsNumbers, resultCube.ColumnsNumbers, c1.ColumnsNumbers.Length);
                Array.Copy(c1.ColumnValues, resultCube.ColumnValues, c1.ColumnsNumbers.Length);
            }
            var c2ValIter = 0;
            for (var i = 1; i <= TotalInputsCount; i++)
            {
                if (!c2.ColumnsNumbers.Contains<int>(i))
                {
                    continue;
                } 
                if (resultCube.ColumnValues[i - 1] == 'x')
                {
                    resultCube.ColumnValues[i - 1] = c2.ColumnValues[c2ValIter];
                    c2ValIter++;
                    continue;
                }
                if (resultCube.ColumnValues[i - 1] != c2.ColumnValues[c2ValIter])
                {                    
                    return null;
                }
                c2ValIter++;
            }

            return resultCube;            
        }

        private Cube CrossDAndSingleCube(Cube dCube, Cube singlCube)
        {
            var c2ValIter = 0;
            Cube resultCube = new Cube();
            resultCube.ColumnsNumbers = new int[TotalInputsCount];
            resultCube.ColumnValues = new char[TotalInputsCount];
            Array.Copy(dCube.ColumnsNumbers, resultCube.ColumnsNumbers, dCube.ColumnsNumbers.Length);
            Array.Copy(dCube.ColumnValues, resultCube.ColumnValues, dCube.ColumnsNumbers.Length);

            for (var i = 1; i <= TotalInputsCount; i++)
            {
                if (!singlCube.ColumnsNumbers.Contains<int>(i))
                {
                    continue;
                }
                if (resultCube.ColumnValues[i - 1] == 'x')
                {
                    resultCube.ColumnValues[i - 1] = singlCube.ColumnValues[c2ValIter];
                    c2ValIter++;
                    continue;
                }
                if (resultCube.ColumnValues[i - 1] == '1' && singlCube.ColumnValues[c2ValIter] == '0')
                {
                    resultCube.ColumnValues[i - 1] = 'd';
                    c2ValIter++;
                    continue;
                }
                if (resultCube.ColumnValues[i - 1] == '0' && singlCube.ColumnValues[c2ValIter] == '1')
                {
                    resultCube.ColumnValues[i - 1] = 'n';
                    c2ValIter++;
                    continue;
                }
                if (resultCube.ColumnValues[i - 1] == '1')
                {
                    resultCube.ColumnValues[i - 1] = '1';
                    c2ValIter++;
                    continue;
                }
                if (resultCube.ColumnValues[i - 1] == '0')
                {
                    resultCube.ColumnValues[i - 1] = '0';
                    c2ValIter++;
                }
            }

            return resultCube;
        }
    }
}
