using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfLanden
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
    public partial class MainWindow : Window
    {
        private static Random random = new Random();
        private DispatcherTimer timer;
        private Stopwatch stopwatch;
        private List<string> landnamen;
        private int beantwoordenvragen;
        private TimeSpan totaletijd;
        private int correctVragen;
        private List<string> gevragdevragen;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += Timer_Tick;
            stopwatch = new Stopwatch();
            landnamen = new List<string> { "Argentinië", "Finland", "Japan", "Marokko", "Nieuw-Zeeland" };
            beantwoordenvragen = 0;
            gevragdevragen = new List<string>();
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            tekstlbl.Content = "";
            StartButton.IsEnabled = false;
            correctVragen = 0;
            beantwoordenvragen = 0;
            totaletijd = TimeSpan.Zero;
            Volgendevraag();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            resultaatlbl.Content = "Fout!";
            Foutgeluid();
            beantwoordenvragen++;
            Volgendevraag();
        }
        private void Volgendevraag()
        {
            if (beantwoordenvragen >= 5)
            {
                Einde();
                return;
            }
            List<string> overblijvendenlanden = landnamen.Except(gevragdevragen).ToList();
            if (overblijvendenlanden.Count == 0)
            {
                gevragdevragen.Clear();
                overblijvendenlanden = landnamen.ToList();
            }
            string landen = overblijvendenlanden[random.Next(overblijvendenlanden.Count)];
            gevragdevragen.Add(landen);
            Vraaglbl.Content = landen;
            stopwatch.Restart();
            timer.Start();
        }
        private void Einde()
        {
            timer.Stop();
            double tijd = totaletijd.TotalSeconds / beantwoordenvragen;
            tekstlbl.Content = $"je had er {correctVragen}/{beantwoordenvragen} juist, Gemiddelde tijd is {tijd:F1} seconden";
            StartButton.IsEnabled = false;
        }
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            stopwatch.Stop();
            totaletijd += stopwatch.Elapsed;

            if (StartButton.IsEnabled == false)
            {
                Image clickedImage = sender as Image;
                string geselectedLand = clickedImage.Tag as string;
                string correctLand = Vraaglbl.Content as string;

                if (geselectedLand == correctLand)
                {
                    resultaatlbl.Content = "Correct!";
                    correctVragen++;
                    Juistgeluid();
                    clickedImage.Opacity = 0.5;
                    gevragdevragen.Add(correctLand);
                }
                else
                {
                    resultaatlbl.Content = "Fout!";
                    Foutgeluid();
                }

                beantwoordenvragen++;
                Volgendevraag();
            }
        }
        private void PlaySound(string filePath)
        {
            SoundPlayer player = new SoundPlayer(filePath);
            player.Load();
            player.Play();
        }
        private void Foutgeluid() => PlaySound("sounds/wrong.wav");
        private void Juistgeluid() => PlaySound("sounds/right.wav");
    }
}