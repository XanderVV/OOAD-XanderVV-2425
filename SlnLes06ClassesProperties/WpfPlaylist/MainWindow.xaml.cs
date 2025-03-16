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

namespace WpfPlaylist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        List<Song> songs = new List<Song>();
      
        public MainWindow()
        {
            InitializeComponent();
            List<Artist> artists = new List<Artist>();
            artists.Add(new Artist("PNL", new DateTime(2014, 1, 1), "PNL is een Franse rapgroep bestaande uit twee broers: Ademo en N.O.S."));
            artists.Add(new Artist("Ludwig van Beethoven", new DateTime(1770, 12, 17), "Ludwig van Beethoven was een Duitse componist. Zijn stijl sluit in zijn vroege periode direct aan op die van Mozart."));
            artists.Add(new Artist("MMZ", new DateTime(2014, 1, 1), "MMZ of 2MZ (Mini Mafia Zoo) is een rapgroep uit de stad Tarterêts in Corbeil-Essonnes."));
            artists.Add(new Artist("DTF", new DateTime(2015, 1, 1), "DTF is een onafhankelijke Franse rapgroep opgericht in 2015"));

            songs.Add(new Song("Für Elise", artists[1], 1824, new TimeSpan(0, 2, 53), new Uri("Mp3/Beethoven-elise.mp3", UriKind.Relative), "Photos/Beethovenhome.jpeg"));
            songs.Add(new Song("J'Comprends pas", artists[0], 2015, new TimeSpan(0, 4, 45), new Uri("Mp3/PNL-jComprends_pas.mp3", UriKind.Relative), "Photos/PNLJComprendspas.jpg"));
            songs.Add(new Song("Mexico", artists[0], 2015, new TimeSpan(0, 4, 05), new Uri("Mp3/PNL-Mexico.mp3", UriKind.Relative), "Photos/PNLMexico.jpg"));
            songs.Add(new Song("Mowgli", artists[0], 2015, new TimeSpan(0, 3, 23), new Uri("Mp3/PNL-MOWGLI.mp3", UriKind.Relative), "Photos/PNLMOWGLI.jpg"));
            songs.Add(new Song("Peace", artists[2], 2015, new TimeSpan(0, 2, 47), new Uri("Mp3/MMZ-Peace.mp3", UriKind.Relative), "Photos/MMZPeace.jpg"));
            songs.Add(new Song("Valar Morghulis", artists[2], 2015, new TimeSpan(0, 3, 39), new Uri("Mp3/MMZ-Valar_Morghulis.mp3", UriKind.Relative), "Photos/MMZValarMorghulis.jpg"));
            songs.Add(new Song("LOIN", artists[3], 2015, new TimeSpan(0, 3, 08), new Uri("Mp3/DTF-LOIN.mp3", UriKind.Relative), "Photos/DTFLOIN.jpg"));
            InitializePlaylist(); // En dit vult de lijst met liedjes
        }
        private void InitializePlaylist()
        {
            foreach (Song song in songs)
            {
                // Voeg de song direct toe aan de ListBox
                playList.Items.Add(song);
            }
        }

        private void PlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playList.SelectedItem is Song selectedSong)
            {
                // We laten de info van het liedje en de artiest zien.
                img.Source = new BitmapImage(new Uri(selectedSong.ImagePath, UriKind.Relative));
                nameArtiste.Text = selectedSong.Artist.Name;
                bornDate.Text = $"bron {selectedSong.Artist.BornDate:dd/MM/yyyy}";
                infoArtiste.Text = selectedSong.Artist.Info;

                playBtn.IsEnabled = true; // De speelknop kan nu ingedrukt worden.
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedItem is Song selectedSong)
            {
                mediaPlayer.Open(new Uri(selectedSong.Uri.ToString(), UriKind.Relative));
                mediaPlayer.Play(); // Start het liedje
                songnametxt.Text = $"now playing: {selectedSong.Name} by {selectedSong.Artist.Name}";

                stopBtn.IsEnabled = true; // Stopknop kan nu gebruikt worden.
                playBtn.IsEnabled = false; // Speelknop kan nu niet gebruikt worden.
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop(); // Stop het liedje.
            stopBtn.IsEnabled = false; // Stopknop kan nu niet gebruikt worden.
            playBtn.IsEnabled = true; // Speelknop kan weer gebruikt worden.
        }
        private void ShuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Items.Count > 0)
            {
                Random rng = new Random();
                int randomIndex = rng.Next(playList.Items.Count);
                playList.SelectedIndex = randomIndex;
                PlaySelectedSong();
            }
        }
        private void PlaySelectedSong()
        {
            if (playList.SelectedItem is Song selectedSong)
            {
                mediaPlayer.Open(new Uri(selectedSong.Uri.ToString(), UriKind.Relative));
                mediaPlayer.Play();
                songnametxt.Text = $"now playing: {selectedSong.Name} by {selectedSong.Artist.Name}";

                img.Source = new BitmapImage(new Uri(selectedSong.ImagePath, UriKind.Relative));
                nameArtiste.Text = selectedSong.Artist.Name;
                bornDate.Text = $"bron {selectedSong.Artist.BornDate:dd/MM/yyyy}";
                infoArtiste.Text = selectedSong.Artist.Info;

                stopBtn.IsEnabled = true; // Stopknop kan nu gebruikt worden.
                playBtn.IsEnabled = false; // Speelknop kan nu niet gebruikt worden.
            }
        }
    }
}