﻿using System.ComponentModel;

namespace DTSXExplorer
{
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fire an event when property changes.
        /// </summary>
        /// <param name="property">Property name.</param>
        protected virtual void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
