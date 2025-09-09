// See https://aka.ms/new-console-template for more information
using System.IO.Ports;
using System.Text;



string portName = "COM3";
int baudrate = 115200;
bool binary = true;
int bytesWritten = 0;

if (args.Length == 0)
{
    Console.WriteLine("Serial dump");
    Console.Write("Enter port name (e.g., COM3): ");

    portName = Console.ReadLine();

    if (string.IsNullOrEmpty(portName))
        portName = "COM3";

    Console.Write("Enter port baudrate (e.g., 115200): ");

    var bratestr = Console.ReadLine();
    if (!int.TryParse(bratestr, out baudrate))
        baudrate = 115200;

    Console.Write("Press Enter to use binary mode or type anything else to use text mode: ");

    var tstr = Console.ReadLine();
    if (!string.IsNullOrEmpty(tstr))
        binary = false;
}
else
{
    if ((args.Length >= 1) && (!string.IsNullOrEmpty(args[0])))
    {
        portName = args[0];
    }

    if (args.Length >= 2)
    {
        if (!int.TryParse(args[1], out baudrate))
            baudrate = 115200;
    }
}

string fileName = $"data_{DateTime.Now:yyyyMMdd_HHmmss}.dat";

try
{
    using (var serialPort = new SerialPort(portName, baudrate, Parity.None, 8, StopBits.One))
    using (var writer = new StreamWriter(fileName, false, Encoding.ASCII))
    {
        serialPort.Encoding = Encoding.ASCII;
        serialPort.Open();
        Console.WriteLine($"Star reading from {portName} {baudrate}, binary={binary}");
        Console.WriteLine("Press [Enter] to finish");
        Console.WriteLine("-----------------------------------------");


        if (binary)
            Console.Write("Bytes read: ");

        bool stop_pending = false;

        Thread inputThread = new Thread(() =>
        {
            Console.ReadLine();
            stop_pending = true;
        });
        inputThread.IsBackground = true;
        inputThread.Start();

        while (!stop_pending)
        {
            if (serialPort.BytesToRead > 0)
            {
                string data = serialPort.ReadExisting();
                writer.Write(data);

                if (binary)
                {
                    bytesWritten += data.Length;
                    Console.CursorLeft = 12;
                    Console.Write(bytesWritten);
                }
                else
                {
                    Console.Write(data);
                }
            }
            Thread.Sleep(10);
        }

        Console.WriteLine($"Data saved to: {fileName}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}