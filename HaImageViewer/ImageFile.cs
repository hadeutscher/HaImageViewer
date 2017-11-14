using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaImageViewer
{
    public class ImageFile : PropertyNotifierBase
    {
        private string path;
        private ObservableCollection<string> categories;

        public ImageFile(string path)
        {
            this.path = path;
            categories = new ObservableCollection<string>();
        }

        public string Path { get { return path; } }
        public ObservableCollection<string> Categories { get { return categories; } }
    }
}
