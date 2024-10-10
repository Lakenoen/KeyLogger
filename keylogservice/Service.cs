using System.ComponentModel;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
class Service : BackgroundService{
    private static TcpServer server =  new TcpServer();
    private static SettingsServer settings = new SettingsServer();
    private static readonly string PathToSettings = AppDomain.CurrentDomain.BaseDirectory + "\\Settings.json";
    private readonly int elipseTime = 1000;
    private static void MakeLogFile(IPAddress? ip){
        if(ip is null)
            return;
        string path = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\" + ip + ".log";
        var fs = new FileStream(path,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.Read);
        fs.Close();
    }
    private static string GetPath(IPAddress? ip){
        if(ip is null)
            return AppDomain.CurrentDomain.BaseDirectory + "\\logs\\unknown.log";
        return  AppDomain.CurrentDomain.BaseDirectory + "\\logs\\" + ip + ".log";
    }
    public static void HookKey(byte[] data,int len,Socket ClientSocket){
        string info = Encoding.Unicode.GetString(data,0,len);
        IPAddress? RemoteIp = ((IPEndPoint?)ClientSocket.RemoteEndPoint)?.Address;
        MakeLogFile(RemoteIp);
        File.AppendAllText(GetPath(RemoteIp),info);
    }
    internal static void Init(){
        if(!File.Exists(settings.PathToErrorLog))
            File.Create(settings.PathToErrorLog);
        if(!File.Exists(PathToSettings))
            File.Create(PathToSettings);
        settings.LoadSettings(PathToSettings);
        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\logs");
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        try{
            Init();
            server.ExecServer(settings,HookKey);
            while(!stoppingToken.IsCancellationRequested){
                await Task.Delay(elipseTime);
            }
        }catch(Exception exep){
            File.AppendAllText(settings.PathToErrorLog,exep.Message + Environment.NewLine);
        }
        finally{
            server.CloseServer();
        }
     }
 }