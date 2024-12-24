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
            i = files.Count - 1;
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
            media.Stop();

            var fileCategories = Database.Get().GetCategories(file_name);
            transitioning = true;
            foreach (Category cat in data.Categories)
            {
                cat.Selected = fileCategories.Contains(cat.Name);
            }
            transitioning = false;
        }
        private void MediaElement_MediaOpened(object sender, EventArgs e)
        {
            if (IsVideo(data.CurrentFileName))
                media.Position = new TimeSpan(0, 0, 0, 0, (int)media.NaturalDuration.TimeSpan.TotalMilliseconds / 2);

            media.Play();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            media.Position = new TimeSpan(0, 0, 1);
            media.Play();
        }

        private bool MatchFilter(string filter, List<string> fileCategories)
        {
            if (filter == "!")
            {
                return fileCategories.Count == 0;
            }
            else if (filter.StartsWith("!"))
            {
                return !fileCategories.Contains(filter.TrimStart('!'));
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
                case Key.D1:
                    TryToggleCategory(0);
                    break;
                case Key.NumPad2:
                case Key.D2:
                    TryToggleCategory(1);
                    break;
                case Key.NumPad3:
                case Key.D3:
                    TryToggleCategory(2);
                    break;
                case Key.NumPad4:
                case Key.D4:
                    TryToggleCategory(3);
                    break;
                case Key.NumPad5:
                case Key.D5:
                    TryToggleCategory(4);
                    break;
                case Key.NumPad6:
                case Key.D6:
                    TryToggleCategory(5);
                    break;
                case Key.NumPad7:
                case Key.D7:
                    TryToggleCategory(6);
                    break;
                case Key.NumPad8:
                case Key.D8:
                    TryToggleCategory(7);
                    break;
                case Key.NumPad9:
                case Key.D9:
                    TryToggleCategory(8);
                    break;
                case Key.NumPad0:
                case Key.D0:
                    TryToggleCategory(9);
                    break;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            media.Pause();
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
