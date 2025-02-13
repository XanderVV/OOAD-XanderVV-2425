using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfTaken
{
    public partial class MainWindow : Window
    {
        private readonly RadioButton[] radioButtons;
        private readonly Stack<ListBoxItem> verwijderdeTaken = new Stack<ListBoxItem>();

        public MainWindow()
        {
            InitializeComponent();
            radioButtons = new RadioButton[] { radiobtn, radiobtn1, radiobtn2 };
        }

        private void CheckForm(object sender, TextChangedEventArgs e)
        {
            CheckForm(); 
        }

        private void CheckForm(object sender, SelectionChangedEventArgs e)
        {
            CheckForm(); 
        }

        private void CheckForm(object sender, RoutedEventArgs e)
        {
            CheckForm(); 
        }

        private bool CheckForm()
        {
            List<string> fouten = new List<string>();

            if (string.IsNullOrEmpty(TaakTextBox.Text))
            {
                fouten.Add("Gelieve een taak in te vullen.");
            }

            if (string.IsNullOrEmpty(PrioriteitComboBox.Text))
            {
                fouten.Add("Gelieve een prioriteit te kiezen.");
            }

            if (DeadlineDatePicker.SelectedDate == null)
            {
                fouten.Add("Gelieve een deadline te kiezen.");
            }

            if (GeselecteerdeRadioButton() == null)
            {
                fouten.Add("Gelieve een door te kiezen.");
            }

            foutlbl.Content = string.Join(Environment.NewLine, fouten);
            foutlbl.Foreground = Brushes.Red;

            return fouten.Count == 0;
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (CheckForm())
            {
                string taakText = TaakTextBox.Text;
                string prioriteitText = PrioriteitComboBox.Text;
                string deadlineText = DeadlineDatePicker.SelectedDate?.ToString("dd/MM/yyyy");
                string doorText = RadioText();

                string itemContent = $"{taakText} ( Deadline: {deadlineText} - Door: {doorText})";
                ListBoxItem newItem = new ListBoxItem
                {
                    Content = itemContent,
                    Background = PrioriteitKleur(prioriteitText)
                };

                TakenListBox.Items.Add(newItem);
                ClearForm();
            }
        }

        private string RadioText() => GeselecteerdeRadioButton()?.Content.ToString() ?? "";
        private RadioButton GeselecteerdeRadioButton() => radioButtons.FirstOrDefault(rb => rb.IsChecked == true);
        private void ResetRadioknoppen() => Array.ForEach(radioButtons, rb => rb.IsChecked = false);

        private void ClearForm()
        {
            TaakTextBox.Text = "";
            PrioriteitComboBox.SelectedIndex = -1;
            DeadlineDatePicker.SelectedDate = null;
            ResetRadioknoppen();
        }

        private SolidColorBrush PrioriteitKleur(string prioriteit)
        {
            switch (prioriteit.ToLower())
            {
                case "laag": return Brushes.Green;
                case "midden": return Brushes.Yellow;
                case "hoog": return Brushes.Red;
                default: return Brushes.White;
            }
        }

        private void Terugzetten_Click(object sender, RoutedEventArgs e)
        {
            if (verwijderdeTaken.Count > 0)
            {
                ListBoxItem terugzetten = verwijderdeTaken.Pop();
                TakenListBox.Items.Add(terugzetten);
            }
        }

        private void Verwijderen_Click(object sender, RoutedEventArgs e)
        {
            if (TakenListBox.SelectedItem is ListBoxItem selectedItem)
            {
                verwijderdeTaken.Push(selectedItem);
                TakenListBox.Items.Remove(selectedItem);
            }
        }
    }
}
