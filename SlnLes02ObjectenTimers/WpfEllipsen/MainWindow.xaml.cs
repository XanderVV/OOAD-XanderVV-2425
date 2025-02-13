using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfEllipsen
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private Random _random = new Random();

        private int _maxEllipsen;
        private int _getekendCount;

        public MainWindow()
        {
            InitializeComponent();

            sldAantalCirkels.ValueChanged += Sliders_ValueChanged;
            sldMinRadius.ValueChanged += Sliders_ValueChanged;
            sldMaxRadius.ValueChanged += Sliders_ValueChanged;

            UpdateLabels();
        }

        private void Sliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (txtAantal != null)
                txtAantal.Text = sldAantalCirkels.Value.ToString("F0");

            if (txtMinRadius != null)
                txtMinRadius.Text = sldMinRadius.Value.ToString("F0");

            if (txtMaxRadius != null)
                txtMaxRadius.Text = sldMaxRadius.Value.ToString("F0");
        }

        private void btnTekenen_Click(object sender, RoutedEventArgs e)
        {
            if (sldMinRadius.Value > sldMaxRadius.Value)
            {
                txtFoutmelding.Text = "De minimum straal mag niet groter zijn dan de maximum straal!";
                txtFoutmelding.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                txtFoutmelding.Visibility = Visibility.Collapsed;
            }

            canvas1.Children.Clear();

            _maxEllipsen = (int)sldAantalCirkels.Value;
            _getekendCount = 0;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50); // 50 ms
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_getekendCount == _maxEllipsen)
            {
                _timer.Stop();
                return;
            }

            double minRadius = sldMinRadius.Value;
            double maxRadius = sldMaxRadius.Value;
            double w = _random.NextDouble() * (maxRadius - minRadius) + minRadius;
            double h = _random.NextDouble() * (maxRadius - minRadius) + minRadius;

            byte r = (byte)_random.Next(256);
            byte g = (byte)_random.Next(256);
            byte b = (byte)_random.Next(256);
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(r, g, b));

            double maxX = canvas1.ActualWidth - w;
            double maxY = canvas1.ActualHeight - h;

            if (double.IsNaN(maxX) || maxX < 0) maxX = canvas1.Width - w;
            if (double.IsNaN(maxY) || maxY < 0) maxY = canvas1.Height - h;

            double xPos = _random.NextDouble() * maxX;
            double yPos = _random.NextDouble() * maxY;

            Ellipse ellipse = new Ellipse()
            {
                Width = w,
                Height = h,
                Fill = brush
            };
            Canvas.SetLeft(ellipse, xPos);
            Canvas.SetTop(ellipse, yPos);
            canvas1.Children.Add(ellipse);

            _getekendCount++;
        }

        private void sldAantalCirkels_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void sldMaxRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
