using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaImageViewer
{
    public class Category : PropertyNotifierBase
    {
        private string name;
        private bool selected;

        public Category(string name)
        {
            this.name = name;
            selected = false;
        }

        public string Name {  get { return name; } }
        public bool Selected {
            get { return selected; }
            set { SetField(ref selected, value); }
        }
    }
}
