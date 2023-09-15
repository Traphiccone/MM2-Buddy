using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MM2Buddy.Models
{
    public class UserSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        private bool _codeSettings;
        public bool CodeSettings
        {
            get { return _codeSettings; }
            set
            {
                if (_codeSettings != value)
                {
                    _codeSettings = value;
                    OnPropertyChanged(nameof(CodeSettings));
                }
            }
        }

        private int _notificationInterval;
        public int NotificationInterval
        {
            get { return _notificationInterval; }
            set
            {
                if (_notificationInterval != value)
                {
                    _notificationInterval = value;
                    OnPropertyChanged(nameof(NotificationInterval));
                }
            }
        }

        // Other properties and OnPropertyChanged implementation
        // ...
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
