using System;

public class MemoryUploaderCtrl : IDisposable
{
    private SerialPort serialPort;
    private int lastAddr = 1;

    public MemoryUploaderCtrl(string port)
	{
        serialPort = new SerialPort();
        serialPort.PortName = portName;
        serialPort.BaudRate = 9600;
        serialPort.Open();
    }

    public void Dispose()
    {
        if (serialPort.IsOpen)
            serialPort.Close();
    }

    string GetVersion()
    {
        serialPort.Write("V\r");
        serialPort.ReadLine();
        string version = serialPort.ReadLine();
        return version;
    }

    void ClearMemory()
    {
        serialPort.Write("C\r");
        serialPort.ReadLine();
    }

    void SendString(string str)
    {
        serialPort.Write($"W ${lastAddr}\r");
        Console.WriteLine(serialPort.ReadLine());
        Console.WriteLine(serialPort.ReadLine());
        var ready = serialPort.ReadLine();

        Console.WriteLine($" Write Ready line => {ready}");
        var rdyParts = ready.TrimEnd('\r').Split(':');
        if (rdyParts.Length <= 1)
            return;

        if (rdyParts[1].Trim() != "WAITING_DATA")
            return;

        Console.WriteLine("Sending data...");
        byte[] strBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(str);
        foreach (var ch in strBytes)
        {
            serialPort.Write($"{ch}\r");
            var dataRcv = serialPort.ReadLine();
            if (dataRcv.StartsWith("ST"))
            {
                var storeStatusParts = dataRcv.TrimEnd('\r').Split(':');
                lastAddr = Convert.ToInt16(storeStatusParts[1].Trim());
                Console.WriteLine(dataRcv);
            }
        }
        serialPort.Write(".\r");
        Console.WriteLine(serialPort.ReadLine());
    }

    public void GetValidPrompt()
    {
        serialPort.Write("\r");
        byte ch;
        while (serialPort.Read(ch, 0, 1) == 0)
            serialPort.Write("\r");
            ;
    }


}
