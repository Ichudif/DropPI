using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropPI
{
    class Item
    {
        public List<Item> Content { get; set; }
        public string Hash { get; set; }
        public string Path { get; set; }

        public Item(List<Item> content, string path, string hash)
        {
            Content = content;
            Path = path;
            Hash = hash;
        }
    }
}
