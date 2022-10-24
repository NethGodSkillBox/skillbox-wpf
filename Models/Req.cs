﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SkillboxWPF.Models
{
    internal class Req : INotifyPropertyChanged
    {
        private string status;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Text { get; set; }
        public string Status
        {
            get { return status; }
            set
            {
                if (status == value)
                    return;

                status = value;

                OnPropertyChanged("Status");
            }
        }
        public DateTime Time { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
