using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;

namespace SupriseLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<BitmapImage> sticks;

        public MainWindow()
        {
            sticks = new ObservableCollection<BitmapImage> { };
            InitializeComponent();
        }

        void LoadPictures()
        {
            //Additional, not required check for my own sanity
            if(!System.IO.Directory.Exists("./Resources"))
            {
                MessageBox.Show("'Resources' folder does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            //Could be done better, consider filtering by built-in EnumerateFiles paremeters 
            var res = System.IO.Directory.EnumerateFiles("./Resources");

            foreach(var path in res)
            {
                if(System.IO.Path.GetExtension(path).ToLower() == ".png")
                {
                    var uri = new Uri(System.IO.Path.GetFullPath(path));
                    //MessageBox.Show(uri.ToString());
                    sticks.Add(new BitmapImage(uri));
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPictures();
            this.DataContext = sticks;
        }
        
        private void OpenImage(object sender, RoutedEventArgs e)
        {
            //Delete all childrens but first from canvas (basically all stickers)
            while(canvas.Children.Count > 1)
            {
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
            }


            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.gif, *.png)|*.jpg;*.jpeg;*.gif;*.png";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                var uri = new Uri(dlg.FileName);
                var bitmap = new BitmapImage(uri);
                loadedImage.Source = bitmap;
            }
            else return;
        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            if (loadedImage.Source == null)
                return;

            //https://stackoverflow.com/a/8883471

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, PixelFormats.Default);

            rtb.Render(canvas);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Png Files (*.png)|*.png";

            var ans = dlg.ShowDialog();
            if(ans == true)
            {
                FileStream fs = null;
                try
                {
                    fs = File.Open(dlg.FileName, FileMode.Create);
                    encoder.Save(fs);
                }
                catch
                {
                    MessageBox.Show("Cannot save file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }

        }

        private void canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Was working correctly w/out it but let it be here
            if (loadedImage.Source == null)
                return;

            //https://stackoverflow.com/questions/21800310/programmatically-add-images-and-position-them-on-wpf-canvas

            var place = e.GetPosition(sender as IInputElement);
            var size = stickerSize.Value;
            var img = stickersList.SelectedItem as BitmapImage;

            var newStick = new Image
            {
                Width = size,
                Height = size,
                Source = img,
            };

            canvas.Children.Add(newStick);
            Canvas.SetTop(newStick, place.Y - size/2);
            Canvas.SetLeft(newStick, place.X - size/2);
        }
    }
}
