using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfButtons
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            CounterButton cb1 = new CounterButton("aantal: ", 1, 10, DirectionType.Up);
            CounterButton cb2 = new CounterButton("aantal: ", 1, 5, DirectionType.Up);
            cb2.Loop = true;
            CounterButton cb3 = new CounterButton("aantal: ", 1, 6, DirectionType.Down);
            stpCounterButtons.Children.Add(cb1);
            stpCounterButtons.Children.Add(cb2);
            stpCounterButtons.Children.Add(cb3);
         
            ColorButton btn1 = new ColorButton(ButtonType.Ok);
            ColorButton btn2 = new ColorButton(ButtonType.Cancel);
            ColorButton btn3 = new ColorButton(ButtonType.No);
            stpColorButtons.Children.Add(btn1);
            stpColorButtons.Children.Add(btn2);
            stpColorButtons.Children.Add(btn3);
        }
    }
}