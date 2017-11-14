using System;
using System.Collections.Generic;
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

        private void SetImage()
        {
            PerhapsSave();
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(files[i]);
            bmp.EndInit();
            data.CurrentImage = bmp;
            var fileCategories = Database.Get().GetCategories(files[i]);
            transitioning = true;
            foreach (Category cat in data.Categories)
            {
                cat.Selected = fileCategories.Contains(cat.Name);
            }
            transitioning = false;
        }

        private bool CategoriesMatch(List<string> fileCategories)
        {
            return filters == null || filters.All(x => fileCategories.Contains(x));
        }

        private IEnumerable<string> FileInfoIterator()
        {
            var files = new DirectoryInfo(System.IO.Path.Combine(folder, "New Folder")).GetFiles().Where(x => imageExtensions.Contains(x.Extension.ToLower())).ToList();
            if (filters != null)
            {
                files = files.Where(x => CategoriesMatch(Database.Get().GetCategories(x.FullName))).ToList();
            }
            files.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));
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
            File.Move(files[i], System.IO.Path.Combine(folder, "Recycle", System.IO.Path.GetFileName(files[i])));
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
            }
        }
    }
}
