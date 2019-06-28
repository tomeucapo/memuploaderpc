using System;
using System.IO.Ports;

namespace MemUploader
{
    public class MemoryUploaderCtrl : IDisposable
    {
        private SerialPort serialPort;
        private int lastAddr = 1;

        public MemoryUploaderCtrl(string portName)
        {
            serialPort = new SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = 9600;
            serialPort.Open();

            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();
        }

        public void Dispose()
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }

        public string GetVersion()
        {
            serialPort.Write("V\r");
            serialPort.ReadLine();
            string version = serialPort.ReadLine();
            return version;
        }

        public bool GetValidPrompt()
        {
            byte[] cr = { 13 };

            serialPort.Write(cr, 0, 1);
            var nextRs = serialPort.ReadLine();
            while (!nextRs.StartsWith(">"))
            {
                serialPort.Write(cr, 0, 1);
                nextRs = serialPort.ReadLine();
                if (nextRs.StartsWith("0"))
                    EndWrite();
            }
            return true;
        }

        public bool ClearMemory()
        {
            GetValidPrompt();
            serialPort.Write("C\r\n");
            var rs=serialPort.ReadLine();

            Console.WriteLine("Clear RS => "+rs);
            return (rs.Trim().Contains("CLEAR"));
        }

        public int SendBlock(int initalAddr, byte[] block, int size)
        {
            GetValidPrompt();
            serialPort.Write($"W {initalAddr}\r\n");

            var ready = serialPort.ReadLine();
            //Console.WriteLine(ready);

            var rdyParts = ready.TrimEnd('\r').Split(':');
            if (rdyParts.Length <= 1)
                return -1;

            if (rdyParts[1].Trim() != "WAITING_DATA")
                return -1;

            //Console.WriteLine($"Sending data... {size}");
            for (int i = 0; i < size; i++)
            {
                //serialPort.Write(block, i, 1);
                serialPort.Write($"{block[i]}\r");
                var ack = serialPort.ReadLine();
                if (!ack.StartsWith("ST"))
                {
                    var storeStatusParts = ack.TrimEnd('\r').Split(':');
                    lastAddr = Convert.ToInt16(storeStatusParts[0].Trim(), 16);
                    if (Convert.ToInt16(storeStatusParts[1].Trim(), 16) != block[i])
                        return -2;
                    Console.WriteLine(ack);
                }
            }

            var dataRcv = serialPort.ReadLine();
            if (dataRcv.StartsWith("ST"))
            {
                var storeStatusParts = dataRcv.TrimEnd('\r').Split(':');
                if (storeStatusParts.Length > 0)
                {
                    var addr = storeStatusParts[1].Split(' ');
                    lastAddr = Convert.ToInt16(addr[0].Trim(),16);
                    var checksum = Convert.ToInt16(addr[1].Trim(), 16);

                    Console.WriteLine(checksum);
                }
                else return -3;
            }

            return lastAddr;
        }

        public void EndWrite()
        {
            serialPort.Write(".\r\n");
            Console.WriteLine(serialPort.ReadLine());
        }
        
    }
}
