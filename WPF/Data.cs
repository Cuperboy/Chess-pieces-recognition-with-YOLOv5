using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using YOLO_csharp;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace WpfApp1
{
    class ModelParams : IModelParams
    {
        public string imagePath { get; set; }
        public string saveDir { get; set; }
        public double precision { get; set; } = 0.5;
        //public Image<Rgba32> result { get; set; }

        public ModelParams(string imagePath, string saveDir)
        {
            this.imagePath = imagePath;
            this.saveDir = saveDir;
        }
    }

    public class Data : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] String propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ObservableCollection<BitmapImage> images { get; set; }

        private string _loadDirPath;
        public string loadDirPath
        {
            get { return _loadDirPath; }
            set
            {
                _loadDirPath = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("loadDirPath"));
            }
        }
        private string _saveDirPath;
        public string saveDirPath
        {
            get { return _saveDirPath; }
            set 
            { 
                _saveDirPath = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("saveDirPath"));
            }
        }

        public double precision_ {  get; set; } = 0.3;
        public double precision
        {
            get { return precision_; }
            set
            {
                precision_ = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("precision"));
            }
        }

        //public Image<Rgba32> result_ { get; set; }
        //public Image<Rgba32> result
        //{
        //    get { return result_; }
        //    set
        //    {
        //        result_ = value;
        //        if (PropertyChanged != null)
        //            PropertyChanged(this, new PropertyChangedEventArgs("result"));
        //    }
        //}

        public void Load(string filename)
        {
            loadDirPath = filename;
        }

        //public void Save(string filename)
        //{
        //    result.Save(filename);
        //}

        public void Process()
        {
            images = new ObservableCollection<BitmapImage>();
            images.Clear();

            var path = new ModelParams(loadDirPath, saveDirPath);
            path.precision = precision;

            var model = new Model(path);
            model.YoloProcess();
            //result = model.YoloProcess();

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(loadDirPath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            images.Add(bitmap);
        }
    }
}
