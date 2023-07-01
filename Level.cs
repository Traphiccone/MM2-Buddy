using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MM2Buddy
{
    public class Level
    {
        private string _code;
        public string Code
        {
            get { return _code; }
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged(nameof(Code));
                }
            }
        }
        public string _name { get; set; }
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string _creator { get; set; }
        public string Creator
        {
            get { return _creator; }
            set
            {
                if (_creator != value)
                {
                    _creator = value;
                    OnPropertyChanged(nameof(Creator));
                }
            }
        }
        public string Link { get; set; }
        public bool AutoOpened { get; set; }
        public bool Active { get; set; }
        public string Hearted { get; set; } //H or B or null
        public string Translation { get; set; } //H or B or null
        public int DeathCnt { get; set; } = 0;
        public DateTime LastPlayed { get; set; }
        public DateTime FirstPlayed { get; set; }
        public bool Logged { get; set; }
        public bool PulledInfo { get; set; }
        public int SMM2InfoSuccess { get; set; } = 1; //1=unsent, 2=sent, 3=success, 4=failed
        public int GoogleTransSuccess { get; set; } = 1; //1=unsent, 2=sent, 3=success, 4=failed
        public bool TransTaskSent { get; set; } = false;
        public Task<string> InfoTask { get; set; }
        public Task<string> TransTask { get; set; }
        public HttpClient HttpClient { get; set; }
        public TimeSpan RecordTime { get; set; }    


        public Level(string code = "No Level Detected", string name = "-", string creator = "-")
        {
            Code = code;
            Name = name;
            Creator = creator;
            Link = "https://smm2.wizul.us/smm2/level/" + code;
            AutoOpened = false;
            Active = false;
            Logged = false;
            PulledInfo = false;
            TransTaskSent = false;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //var handlers = PropertyChanged;

            //handlers(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
