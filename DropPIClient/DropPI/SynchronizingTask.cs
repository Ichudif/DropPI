using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropPI
{
    class SynchronizingTask
    {
        public string Folder { get; set; }
        public DateTime Schedule { get; set; }
        public int ID { get; set; }

        private static int SERVER_PORT = 12345;

        public SynchronizingTask(string folder, DateTime schedule, int id)
        {
            Folder = folder;
            Schedule = schedule;
            ID = id;
        }

        public void CheckAndSyncFolders()
        {
            TcpClient tcp = new TcpClient();
            tcp.Connect("192.168.1.122", SERVER_PORT);
            Stream writer = tcp.GetStream();

            LogClass.Log("starting ...");
            Stopwatch sw = new Stopwatch();
            sw.Start();


            string mainpath = Folder.Split(new string[] { Folder.Split('\\').Last() }, StringSplitOptions.None)[0];
            Item Tree = GetHierachy(Folder);
            List<Item> FlattenedTree = Flatten(Tree);

            List<string> NotChanged = new List<string>();
            foreach (Item item2 in FlattenedTree)
            {
                if (NotChanged.All(path => !item2.Path.Contains(path)))
                {
                    string ServerHash = GetHash(item2.Path.Split(new string[] { mainpath }, StringSplitOptions.None)[1], writer);
                    LogClass.Log("Real Hash is: " + item2.Hash);
                    if (item2.Hash.ToLower().Trim() != ServerHash.ToLower().Trim())
                    {
                        if (File.Exists(item2.Path))
                        {
                            LogClass.Log("Uploading File: " + item2.Path);
                            bool help = UploadFile(item2.Path.Split(new string[] { mainpath }, StringSplitOptions.None)[1], item2.Path, writer);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (Directory.Exists(item2.Path))
                        {
                            NotChanged.Add(item2.Path + "\\");
                        }
                    }
                }
                Thread.Sleep(10);
            }
            Thread.Sleep(10);


            writer.Close();
            tcp.Close();
            sw.Stop();

            LogClass.Log("Finished ... took " + sw.Elapsed.ToString());
            MessageBox.Show("Finished Syncing...");
        }

        private List<Item> Flatten(Item items)
        {
            List<Item> returnme = new List<Item>();
            returnme.Add(items);
            foreach (Item item in items.Content)
            {
                if (item.Content != null)
                {
                    returnme.AddRange(Flatten(item));
                }
                else
                {
                    returnme.Add(item);
                }
            }

            returnme.ForEach(item => item.Content = null);
            return returnme;
        }

        private Item GetHierachy(string Path)
        {
            List<Item> FolderContents = new List<Item>();
            var md5var = MD5.Create();

            foreach (string item in Directory.GetDirectories(Path))
            {
                Item cont = GetHierachy(item);

                FolderContents.Add(cont);
            }

            foreach (var item in Directory.GetFiles(Path))
            {
                using (var stream = File.OpenRead(item))
                {
                    FolderContents.Add(new Item(null, item, BitConverter.ToString(md5var.ComputeHash(stream)).Replace("-", "").ToLowerInvariant()));
                }
            }

            string hashacumulated = "";
            FolderContents.OrderBy(item => item.Hash).ToList().ForEach(item2 => hashacumulated += item2.Hash);

            var hash = md5var.ComputeHash(System.Text.Encoding.ASCII.GetBytes(hashacumulated));
            string hashstring = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return new Item(FolderContents, Path, hashstring);
        }

        private bool UploadFile(string relativePath, string absolutePath, Stream writer)
        {
            FileInfo filetosend = new FileInfo(absolutePath);

            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(filetosend.Length));
            bytes.Reverse();

            byte[] newbytes = new byte[5];

            string sendme = "1" + relativePath.Length.ToString().PadLeft(3, '0') + "00000" + relativePath;
            byte[] ba = new byte[sendme.Length];
            for (int i = 0; i < sendme.Length; i++)
            {
                ba[i] = Convert.ToByte((int)sendme[i]);
            }

            ba[4] = bytes[3];
            ba[5] = bytes[4];
            ba[6] = bytes[5];
            ba[7] = bytes[6];
            ba[8] = bytes[7];

            writer.Write(ba, 0, ba.Length);

            using (FileStream reader = new FileStream(absolutePath, FileMode.Open, FileAccess.Read))
            {
                int value = 1600;
                int count = 0;
                for (int i = 0; i < filetosend.Length; i += value)
                {
                    count++;
                    byte[] writeme = new byte[value];
                    if (filetosend.Length - i < value)
                    {
                        value = (int)filetosend.Length - i;
                    }
                    reader.Read(writeme, 0, value);

                    writer.Write(writeme, 0, value);


                    int back = int.Parse(((char)writer.ReadByte()).ToString());
                    if (back == 1)
                    {
                        LogClass.Log("1 Byte Back");
                    }
                    else
                    {
                        LogClass.Log("Answer: " + back);
                    }
                }
            }

            int h = writer.ReadByte();
            int answer = int.Parse(((char)h).ToString());

            Thread.Sleep(1);

            if (answer == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetHash(string Path, Stream writer)
        {
            //try
            //{
            string sendme = "0" + Path.Length.ToString().PadLeft(3, '0') + Path;

            byte[] ba = new byte[sendme.Length];
            for (int i = 0; i < sendme.Length; i++)
            {
                ba[i] = Convert.ToByte((int)sendme[i]);
            }

            writer.Write(ba, 0, ba.Length);

            string returnme = "";
            byte[] buffer = new byte[32];
            int length = writer.Read(buffer, 0, 32);
            buffer.ToList().ForEach(item => returnme += Convert.ToChar(item));
            LogClass.Log("Path: " + Path + "\n Server Hash: " + returnme);

            return returnme;
            //}
            //catch (Exception ex)
            //{
            //    return "";
            //}
        }
    }
}
