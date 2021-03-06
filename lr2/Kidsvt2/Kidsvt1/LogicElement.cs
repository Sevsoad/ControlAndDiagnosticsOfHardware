﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kidsvt1
{
    class LogicElement
    {
        public LogicElement Right { get; set; }

        public LogicElement[] Left { get; set; }

        public bool IsResult { get; set; }

        public Func<bool[], bool> LogicOperation {get; set;}

        public int Number { get; set; }

        public int[] InputNumbers { get; set; }

        public bool IsStart { get; set; }

        public List<Cube> SingularCubes { get; set; }

        public List<Cube> DCubes { get; set; }
    }
}
