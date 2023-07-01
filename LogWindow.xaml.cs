using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MM2Buddy
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
        }
        public void SetLogData(IEnumerable<LogEntry> logEntries)
        {
            var stringBuilder = new StringBuilder();
            foreach (var entry in logEntries)
            {
                stringBuilder.AppendLine($"[{entry.Timestamp}] {entry.Message}");
            }

            txtLog.Text = stringBuilder.ToString();
        }
    }
}
