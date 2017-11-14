using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HaImageViewer
{
    public class Data : PropertyNotifierBase
    {
        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories { get { return categories ?? (categories = new ObservableCollection<Category>()); } }

        private ImageSource currentImage;
        public ImageSource CurrentImage {
            get { return currentImage ?? (currentImage = new BitmapImage()); }
            set { SetField(ref currentImage, value); }
        }
    }
}
