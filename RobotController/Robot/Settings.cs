using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Shapes;

namespace RobotController.Robot
{
    public class Settings
    {
        FileKeyStore store;

        public Settings()
        {
            store = new FileKeyStore("settings");
        }

        public string readString(string key, string defaul)
        {
            string read = store.read(key);

            if(read == "")
            {
                return defaul;
            }
            else
            {
                return read;
            }
        }

        public void saveString(string key, string value)
        {
            store.write(key, value);
            store.save();
        }
    }

    public class FileKeyStore
    {
        private string filename = "";
        private string extension = ".fks";

        public Dictionary<string, string> cache;
        public FileKeyStore(string filename)
        {
            this.filename = filename;
            load();
        }

        public string read(string key)
        {
            if(cache.ContainsKey(key))
            {
                return cache[key];
            }
            else
            {
                return "";
            }
        }

        public void write(string key, string value)
        {
            if(value.Contains("|") || key.Contains("|"))
            {
                return;
            }

            if(cache.ContainsKey(key))
            {
                cache[key] = value;
            }
            else
            {
                cache.Add(key, value);
            }
        }

        public void load()
        {
            if(File.Exists(filename + extension))
            {
                cache = new Dictionary<string, string>();
                string[] lines = File.ReadAllLines(filename + extension);

                for(int i = 0; i < lines.Length; i++)
                {
                    string[] split = lines[i].Split("|");

                    if(split.Length == 2)
                    {
                        cache.Add(split[0], split[1]);
                    }
                }

                Trace.WriteLine(filename + extension + " Read: " + lines.Length + " Loaded: " + cache.Count);
            }
        }

        public void save()
        {
            List<string> lines = new List<string>(cache.Count);

            foreach ((string k, string v) in cache)
            {
                lines.Add(k + "|" + v);
            }

            if(lines.Count > 0)
            {
                File.WriteAllLines(filename + extension, lines.ToArray());
            }
        }
    }
}
