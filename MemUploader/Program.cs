using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;

namespace MemUploader
{
    class Program
    {
        
        static void Main(string[] args)
        {
            using (MemoryUploaderCtrl memCtrl = new MemoryUploaderCtrl("COM4"))
            {   
                if (memCtrl.ClearMemory())
                    Console.WriteLine("Memory cleared!");
                
                var initialAddress = 128;
                foreach (string line in File.ReadLines(@"C:\Users\tomeu\OneDrive\Retro\dragon\proves\retromallorca.hex"))
                {
                    Console.WriteLine(line);
                    var block = new byte[8];
                    int i = 0;
                    foreach (var strByte in line.Split(','))
                    {
                        block[i] = Convert.ToByte(strByte, 16);
                        i++;
                    }
                    var st = memCtrl.SendBlock(initialAddress, block, 8);
                    if (st < 0)
                    {
                        Console.WriteLine(st);
                        break;
                    }
                    initialAddress+=8;
                }
                memCtrl.EndWrite();
                
                //memCtrl.SendString("HELLO WORLD!");
            }
        }
    }
}
    