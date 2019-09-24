using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Patterns
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public bool Paused
        {
            get; set;
        } = true;
        private const string IMAGEPATH = @"..\..\Images\Moire";
        private double rotation = 0.0;
        private Image drag = null;
        private Point mousePos;
        private Vector offOrigin = new Vector(0, 0);

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Update();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public async void Update()
        {
            while (true)
            {
                rotation = Paused ? 0 : Scale.Value * Speedometer.Value;
                TransformGroup transform = new TransformGroup();
                transform.Children.Add(Fore.RenderTransform);
                transform.Children.Add(new RotateTransform(rotation, Fore.ActualWidth / 2 + offOrigin.X, Fore.ActualHeight / 2 + offOrigin.Y));
                Fore.RenderTransform = transform;
                await Task.Delay(TimeSpan.FromMilliseconds(25));
            }
        }

        #region Controls
        private void Refresh(object sender, RoutedEventArgs e)
        {
            TransformGroup transform = new TransformGroup();
            transform.Children.Add(Fore.RenderTransform);
            transform.Children.Add(new TranslateTransform(-offOrigin.X, -offOrigin.Y));
            Fore.RenderTransform = transform;
            offOrigin -= offOrigin;
        }

        private void Toggle(object sender, RoutedEventArgs e)
        {
            Paused = !Paused;
            OnPropertyChanged("Paused");
        }

        private void ViewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source is Image png)
            {
                mousePos = e.GetPosition(View);
                drag = png;
                View.CaptureMouse();
            }
        }

        private void ViewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (drag != null)
            {
                drag = null;
                View.ReleaseMouseCapture();
            }
        }

        private void ViewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (drag != null)
            {
                Point pos = e.GetPosition(View);
                Vector offset = pos - mousePos;
                mousePos = pos;
                offOrigin += offset;
                TransformGroup transform = new TransformGroup();
                transform.Children.Add(Fore.RenderTransform);
                transform.Children.Add(new TranslateTransform(offset.X, offset.Y));
                Fore.RenderTransform = transform;
            }
        }
        #endregion

        #region Loading
        private void LoadBackground(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = CreateDialog("Images|*.png;*.gif",
                                                 Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), IMAGEPATH)),
                                                 false);
            if (Wasopened(dialog))
            {
                Back.Source = new BitmapImage(new Uri(MatchPattern(dialog.FileName, "Images.*").Value, UriKind.Relative));
            }
        }

        private void LoadForeground(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = CreateDialog("Images|*.png;*.gif",
                                                 Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), IMAGEPATH)),
                                                 false);
            if (Wasopened(dialog))
            {
                Fore.Source = new BitmapImage(new Uri(MatchPattern(dialog.FileName, "Images.*").Value, UriKind.Relative));
            }
        }

        private Match MatchPattern(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);
            return match;
        }

        private OpenFileDialog CreateDialog(string filter, string directory, bool select)
        {
            return new OpenFileDialog
            {
                Filter = filter,
                InitialDirectory = directory,
                Multiselect = select
            };
        }
        private bool Wasopened(OpenFileDialog dialog)
        {
            bool? opened = dialog.ShowDialog();
            return opened.HasValue && opened.Value;
        }
        #endregion
    }
}
