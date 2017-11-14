using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaImageViewer
{
    public class Database
    {
        private string path;
        private static Database instance = null;
        public static Database Get()
        {
            if (instance == null)
            {
                instance = new Database("db.bin");
            }
            return instance;
        }

        private Dictionary<string, List<string>> database;

        public Database(string path)
        {
            this.path = path;
            database = new Dictionary<string, List<string>>();
            string text = File.Exists(path) ? File.ReadAllText(path) : "";
            foreach (string line in text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var args = line.Split(new string[] { "," }, StringSplitOptions.None);
                database[args[0]] = args.Skip(1).ToList();
            }
        }

        public List<string> GetCategories(string path)
        {
            List<string> value;
            return database.TryGetValue(path, out value) ? value : new List<string>();
        }

        public void SetCategories(string path, List<string> categories)
        {
            database[path] = categories;
        }

        public void Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, List<string>> entry in database)
            {
                sb.AppendLine(entry.Value.Count > 0 ? (entry.Key + "," + entry.Value.Aggregate((x, y) => x + "," + y)) : entry.Key);
            }
            File.WriteAllText(path, sb.ToString());
        }
    }
}
