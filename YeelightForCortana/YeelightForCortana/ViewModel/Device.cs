﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightForCortana.ViewModel
{
    public class Device : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
