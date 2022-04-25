using NReco.VideoConverter;
using NReco.VideoInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HaImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand MoveNext = new RoutedCommand();
        public static RoutedCommand MovePrev = new RoutedCommand();
        public static RoutedCommand MoveNextBig = new RoutedCommand();
        public static RoutedCommand MovePrevBig = new RoutedCommand();
        public static RoutedCommand Delete = new RoutedCommand();

        private static string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private static string[] videoExtensions = { ".mp4", ".webm", ".wmv", ".m4v", ".flv", ".avi" };

        private string folder;
        private IList<string> files;
        private int i;
        private Data data;
        private List<string> filters;
        private bool transitioning = false;
        private FFMpegConverter ffMpeg = new FFMpegConverter();
        private FFProbe ffProbe = new FFProbe();

        static MainWindow()
        {
            MovePrev.InputGestures.Add(new KeyGesture(Key.Left));
            MoveNext.InputGestures.Add(new KeyGesture(Key.Right));
            MovePrevBig.InputGestures.Add(new KeyGesture(Key.Left, ModifierKeys.Control));
            MoveNextBig.InputGestures.Add(new KeyGesture(Key.Right, ModifierKeys.Control));
            Delete.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        public MainWindow(string folder, List<string> filters = null)
        {
            this.folder = folder;
            this.filters = filters;
            InitializeComponent();
            data = new Data();
            DataContext = data;
            files = FileInfoIterator().ToList();
            if (files.Count == 0)
            {
                return;
            }
            i = 0;
            CategoriesIterator().ToList().ForEach(x => data.Categories.Add(new Category(x)));
            SetImage();
        }

        private DateTime lastSave = DateTime.Now;
        private void PerhapsSave()
        {
            if (DateTime.Now - lastSave > TimeSpan.FromSeconds(10))
            {
                Database.Get().Save();
                lastSave = DateTime.Now;
            }
        }

        public static bool IsImage(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLower();
            return imageExtensions.Contains(extension);
        }

        public static bool IsVideo(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLower();
            return videoExtensions.Contains(extension);
        }

        private void SetImage()
        {
            PerhapsSave();

            string file_name = files[i];
            data.CurrentFileName = file_name;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            if (IsImage(file_name))
            {
                bmp.UriSource = new Uri(file_name);
            }
            else if (IsVideo(file_name))
            {
                var stream = new MemoryStream();
                try
                {
                    ffProbe.IncludeStreams = false;
                    var info = ffProbe.GetMediaInfo(file_name);
                    ffMpeg.GetVideoThumbnail(file_name, stream, (float)info.Duration.TotalSeconds / 2);
                    stream.Seek(0, SeekOrigin.Begin);
                    if (stream.Length > 0)
                    {
                        bmp.StreamSource = stream;
                    }
                    else
                    {
                        bmp = null;
                    }
                } catch (FFMpegException)
                {
                    bmp = null;
                }
                catch (FFProbeException)
                {
                    bmp = null;
                }
            }
            else
            {
                bmp = null;
            }

            if (bmp == null)
            {
                bmp = new BitmapImage(new Uri("pack://application:,,,/HaImageViewer;component/Resources/Empty.png"));
            }
            else
            {
                bmp.EndInit();
            }
            data.CurrentImage = bmp;
            var fileCategories = Database.Get().GetCategories(file_name);
            transitioning = true;
            foreach (Category cat in data.Categories)
            {
                cat.Selected = fileCategories.Contains(cat.Name);
            }
            transitioning = false;
        }

        private bool MatchFilter(string filter, List<string> fileCategories)
        {
            if (filter == "!")
            {
                return fileCategories.Count == 0;
            }
            else
            {
                return fileCategories.Contains(filter);
            }
        }

        private bool CategoriesMatch(List<string> fileCategories)
        {
            return filters == null || filters.All(x => MatchFilter(x, fileCategories));
        }

        private DateTime GetFileTime(FileInfo fi)
        {
            return (fi.LastWriteTime < fi.CreationTime) ? fi.LastWriteTime : fi.CreationTime;
        }

        private IEnumerable<string> FileInfoIterator()
        {
            var files = new DirectoryInfo(System.IO.Path.Combine(folder, "New Folder")).GetFiles().Where(x => imageExtensions.Contains(x.Extension.ToLower()) || videoExtensions.Contains(x.Extension.ToLower())).ToList();
            if (filters != null)
            {
                files = files.Where(x => CategoriesMatch(Database.Get().GetCategories(x.FullName))).ToList();
            }
            files.Sort((x, y) => GetFileTime(x).CompareTo(GetFileTime(y)));
            foreach (FileInfo file in files)
            {
                yield return file.FullName;
            }
        }

        private IEnumerable<string> CategoriesIterator()
        {
            var folders = new DirectoryInfo(folder).GetDirectories().Where(x => x.Name != "New folder" && x.Name != "Recycle").Select(x => x.Name).ToList();
            folders.Sort();
            return folders;
        }

        int pmod(int x, int m)
        {
            return (x % m + m) % m;
        }

        private void MovePrev_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            i = pmod(i - 1, files.Count);
            SetImage();
        }

        private void MoveNext_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            i = pmod(i + 1, files.Count);
            SetImage();
        }

        private void MovePrevBig_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            i = pmod(i - 10, files.Count);
            SetImage();
        }

        private void MoveNextBig_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            i = pmod(i + 10, files.Count);
            SetImage();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!transitioning)
                Database.Get().SetCategories(files[i], data.Categories.Where(x => x.Selected).Select(x => x.Name).ToList());
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var recycle = System.IO.Path.Combine(folder, "Recycle");
            if (!Directory.Exists(recycle))
                Directory.CreateDirectory(recycle);
            File.Move(files[i], System.IO.Path.Combine(recycle, System.IO.Path.GetFileName(files[i])));
            files.RemoveAt(i);
            i = pmod(i, files.Count);
            SetImage();
        }

        private void TryToggleCategory(int index)
        {
            if (index < data.Categories.Count)
            {
                data.Categories[index].Selected = !data.Categories[index].Selected;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.NumPad1:
                    TryToggleCategory(0);
                    break;
                case Key.NumPad2:
                    TryToggleCategory(1);
                    break;
                case Key.NumPad3:
                    TryToggleCategory(2);
                    break;
                case Key.NumPad4:
                    TryToggleCategory(3);
                    break;
                case Key.NumPad5:
                    TryToggleCategory(4);
                    break;
                case Key.NumPad6:
                    TryToggleCategory(5);
                    break;
                case Key.NumPad7:
                    TryToggleCategory(6);
                    break;
                case Key.NumPad8:
                    TryToggleCategory(7);
                    break;
                case Key.NumPad9:
                    TryToggleCategory(8);
                    break;
                case Key.NumPad0:
                    TryToggleCategory(9);
                    break;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(files[i]);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (files.Count == 0)
            {
                Console.WriteLine("No files");
                Close();
            }
        }
    }
}
