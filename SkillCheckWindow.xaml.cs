using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SkillCheckApp
{
    public partial class SkillCheckWindow : Window
    {
        // how wide is the hit‐zone (in degrees)
        const double HitZoneSweep = 15;
        // how wide is the spinning indicator
        const double IndicatorSweep = 4;
        // spin speed (degrees per second)
        const double SpinSpeed = 180;

        readonly Random _rand = new Random();
        double _hitStart, _hitEnd;
        double _currentAngle;
        Point _center;
        double _radius;
        TimeSpan _lastRenderTime;

        public SkillCheckWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            CompositionTarget.Rendering += OnRendering;

            // play sound
            var player = new MediaPlayer();
            player.Open(new Uri(@"./skill_check_start.mp3", UriKind.Relative));
            player.Play();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // compute center & radius once canvas is measured
            _radius = CircleCanvas.ActualWidth / 2.0;
            _center = new Point(_radius, _radius);

            // pick a random start for this hit-zone
            _hitStart = _rand.NextDouble() * 360;
            _hitEnd = (_hitStart + HitZoneSweep) % 360;

            // draw the static hit-zone arc
            HitZoneArc.Data = CreateArc(_center, _radius, _hitStart, _hitEnd);

            // prime lastRenderTime so first frame skips
            _lastRenderTime = TimeSpan.Zero;

            // grab focus for spacebar
            Focus();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_COMMAND = 0x0111;
        private const int MIN_ALL = 419;

        public static void MinimizeAllWindows()
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
        }

        void OnRendering(object sender, EventArgs e)
        {
            // get the rendering timestamp
            var args = (RenderingEventArgs)e;
            if (_lastRenderTime == TimeSpan.Zero)
            {
                _lastRenderTime = args.RenderingTime;
                return;
            }

            // compute delta
            double dt = (args.RenderingTime - _lastRenderTime).TotalSeconds;
            _lastRenderTime = args.RenderingTime;

            // advance angle & wrap
            _currentAngle = (_currentAngle + SpinSpeed * dt) % 360;

            // redraw the indicator arc
            double start = _currentAngle - IndicatorSweep / 2;
            double end = _currentAngle + IndicatorSweep / 2;
            IndicatorArc.Data = CreateArc(_center, _radius, start, end);
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space) return;

            // did we hit inside the random hit-zone?
            bool inside = (_hitStart < _hitEnd)
                ? _currentAngle >= _hitStart && _currentAngle <= _hitEnd
                : _currentAngle >= _hitStart || _currentAngle <= _hitEnd;

            if (inside)
            {
                var player = new MediaPlayer();
                player.Open(new Uri(@"./skill_check_success.mp3", UriKind.Relative));
                player.Play();
                CompositionTarget.Rendering -= OnRendering;
                Close();
            }
            else
            {
                // Optional: flash the hit-zone red for feedback
                var player = new MediaPlayer();
                player.Open(new Uri(@"./skill_check_fail.mp3", UriKind.Relative));
                player.Play();
                MinimizeAllWindows();
            }
        }

        // builds a simple arc path (no pie-slice)
        PathGeometry CreateArc(Point center, double radius, double startDeg, double endDeg)
        {
            double r = radius;
            double a1 = startDeg * Math.PI / 180.0;
            double a2 = endDeg * Math.PI / 180.0;

            var p1 = new Point(center.X + r * Math.Cos(a1),
                               center.Y + r * Math.Sin(a1));
            var p2 = new Point(center.X + r * Math.Cos(a2),
                               center.Y + r * Math.Sin(a2));

            bool largeArc = Math.Abs(endDeg - startDeg) > 180;

            var fig = new PathFigure { StartPoint = p1, IsClosed = false };
            fig.Segments.Add(new ArcSegment(
                p2,
                new Size(r, r),
                0,
                largeArc,
                SweepDirection.Clockwise,
                true));

            return new PathGeometry(new[] { fig });
        }
    }
}
