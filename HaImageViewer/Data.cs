﻿using System;
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
        public ObservableCollection<Category> Categories
        {
            get { return categories ?? (categories = new ObservableCollection<Category>()); }
        }

        private string currentFileName;
        public string CurrentFileName
        {
            get { return currentFileName ?? (currentFileName = ""); }
            set { SetField(ref currentFileName, value); }
        }

        private bool categoriesVisible = true;
        public bool CategoriesVisible
        {
            get { return categoriesVisible; }
            set { SetField(ref categoriesVisible, value); }
        }
    }
}
