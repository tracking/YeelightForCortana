﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class Device : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Power { get; set; }
        public bool Online { get; set; }

        public Device()
        {

        }
    }
}
