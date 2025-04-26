using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SkillCheckApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // fire every 5 minutes
        private readonly DispatcherTimer _scheduleTimer = new DispatcherTimer();

        private readonly Random _rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
            _scheduleTimer.Tick += OnScheduleTimerTick;
            SetRandomInterval();
            _scheduleTimer.Start();
        }

        private void SetRandomInterval()
        {
            // example: random between 2 and 5 minutes
            double minutes = _rand.NextDouble() * (5.0 - 2.0) + 2.0;
            
            _scheduleTimer.Interval = TimeSpan.FromMinutes(minutes);
        }

        private void OnScheduleTimerTick(object sender, EventArgs e)
        {
            _scheduleTimer.Stop();

            var skillCheck = new SkillCheckWindow();
            skillCheck.Closed += (_, __) =>
            {
                SetRandomInterval();
                _scheduleTimer.Start();
            };
            skillCheck.Show();
        }
    }
}