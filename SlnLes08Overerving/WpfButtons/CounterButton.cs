using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfButtons
{
    public class CounterButton : Button
    {
        public string Prefix { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Count { get; set; }
        public DirectionType Direction { get; set; }
        public bool Loop { get; set; }

        public CounterButton(string prefix, int min, int max, DirectionType direction)
        {
            Prefix = prefix;
            Min = min;
            Max = max;
            Direction = direction;
            Loop = false;
            
            Count = (Direction == DirectionType.Up) ? Min : Max;
            
            this.Width = 100;
            this.Height = 35;
            this.Margin = new Thickness(5);
            this.Background = new SolidColorBrush(Colors.LightGray);
            this.BorderThickness = new Thickness(1);
            this.BorderBrush = new SolidColorBrush(Colors.Gray);
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
            
            UpdateButtonText();
            
            this.MouseRightButtonDown += CounterButton_MouseRightButtonDown;
        }

        private void CounterButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Reset();
            e.Handled = true;
        }

        public void Reset()
        {
            Count = (Direction == DirectionType.Up) ? Min : Max;
            UpdateButtonText();
        }

        protected override void OnClick()
        {
            if (Direction == DirectionType.Up)
            {
                Count++;
                if (Count > Max)
                {
                    if (Loop)
                    {
                        Count = Min;
                    }
                    else
                    {
                        Count = Max;
                    }
                }
            }
            else 
            {
                Count--;
                if (Count < Min)
                {
                    if (Loop)
                    {
                        Count = Max;
                    }
                    else
                    {
                        Count = Min;
                    }
                }
            }

            UpdateButtonText();
            base.OnClick();
        }

        private void UpdateButtonText()
        {
            this.Content = $"{Prefix}{Count}";
        }
    }
} 