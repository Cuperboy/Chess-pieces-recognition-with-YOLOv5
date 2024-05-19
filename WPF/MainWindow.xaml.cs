using Microsoft.Win32;
using System.Diagnostics;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public Data data;
        public MainWindow()
        {
            InitializeComponent();
            data = new Data();
            DataContext = data;
        }

        //private void Data_Save_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveFileDialog saveFileDialog = new SaveFileDialog();
        //    saveFileDialog.DefaultExt = ".jpg";
        //    saveFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
        //    saveFileDialog.CreatePrompt = true;
        //    if (saveFileDialog.ShowDialog() == false)
        //    {
        //        MessageBox.Show("Saving Error");
        //        return;
        //    }
        //    else
        //    {
        //        string FilePath = saveFileDialog.FileName;
        //        saveFileDialog.DefaultExt = ".jpg";
        //        saveFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";

        //        data.Save(FilePath);
        //        MessageBox.Show("You saved in: " + FilePath);
        //    }
        //}

        private void Data_Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".jpg";
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
            if (openFileDialog.ShowDialog() == false)
            {
                MessageBox.Show("Loading Error");
                return;
            }
            else
            {
                string FilePath = openFileDialog.FileName;
                try
                {
                    data.Load(FilePath);
                    MessageBox.Show("You loaded from: " + FilePath);
                    Image_Update(FilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
        public void Image_Update(string FilePath)
        {
            var uri = new Uri(FilePath);
            var bitmap = new BitmapImage(uri);
            image1.Source = bitmap;
        }

        private void Image_Recognition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                data.Process();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show("Recognition Success!");
        }

        private void Show_Result_Click(object sender, RoutedEventArgs e)
        {
            Image_Update("F:\\ML\\PracticalML2024\\new\\result.jpg");
        }
    }
}