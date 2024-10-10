
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

class SettingsServer{
    public string Ip{get;set;} = "127.0.0.1";
    public  ushort Port{get;set;} = 11000;
    public  int MaxConnection{get;set;} = 0xFF;
    public  string PathToErrorLog{get;set;} = AppDomain.CurrentDomain.BaseDirectory + "\\ErrorLog.txt";
    public void LoadSettings(in string path){
        string strSettings = File.ReadAllText(path);
        SettingsServer? TempSet = JsonSerializer.Deserialize<SettingsServer>(strSettings);
        if(TempSet is null)
            return;
        Ip = TempSet.Ip;
        Port = TempSet.Port;
        MaxConnection = TempSet.MaxConnection;
        PathToErrorLog = TempSet.PathToErrorLog;
    }
}