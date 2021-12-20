using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;


namespace WpfAppDKab
{
    public partial class MainWindow : Window
    {
        Comp Comp = new Comp();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Comp;

        }
        private void Open(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Comp.InputPath = dialog.SelectedPath;
                }
            }
        }
        private void Start(object sender, RoutedEventArgs e)
        {
            Comp.StartProc();
        }
        private void Stop(object sender, RoutedEventArgs e)
        {
            Comp.StopProc();
        }
        private void Reset(object sender, RoutedEventArgs e)
        {
            Comp.Reset();
        }
        public void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            Comp.SelectorHandler(((System.Windows.Controls.ListBox)sender).SelectedItem as string);
        }
    }
}

