using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfButtons
{
    public class ColorButton : Button
    {
        public ButtonType ButtonType { get; private set; }

        public ColorButton(ButtonType buttonType)
        {
            ButtonType = buttonType;
            
            ApplyButtonStyle();
            
            this.Width = 200;
            this.Height = 70;
            this.FontSize = 24;
            this.FontWeight = FontWeights.Bold;
            this.Margin = new Thickness(10);
            this.BorderThickness = new Thickness(2);
        }
        
        private void ApplyButtonStyle()
        {
            switch (ButtonType)
            {
                case ButtonType.Ok:
                    this.Content = "Ok";
                    this.Background = new SolidColorBrush(Colors.LightGreen);
                    this.Foreground = new SolidColorBrush(Colors.DarkGreen);
                    this.BorderBrush = new SolidColorBrush(Colors.Green);
                    break;
                case ButtonType.Cancel:
                    this.Content = "Annuleren";
                    this.Background = new SolidColorBrush(Colors.LightBlue);
                    this.Foreground = new SolidColorBrush(Colors.DarkBlue);
                    this.BorderBrush = new SolidColorBrush(Colors.Blue);
                    break;
                case ButtonType.No:
                    this.Content = "Nee";
                    this.Background = new SolidColorBrush(Colors.LightPink);
                    this.Foreground = new SolidColorBrush(Colors.DarkRed);
                    this.BorderBrush = new SolidColorBrush(Colors.Red);
                    break;
            }
        }
    }
} 