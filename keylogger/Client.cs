using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Text;

class TcpClient{
    private enum SendType{
        STREAM,
        BLOCK,
    };
    private SendType SendTypeObj{get;set;} = SendType.BLOCK;
    private const uint MaxBuffSize = 1 << 5;
    private const uint TimeToTryConnect = 1000*3;
    private StringBuilder buffer{get;set;} = new StringBuilder();
    private object BufferLocker{get;set;} = new();
    private object SendTypeLocker{get;set;} = new();
    private Socket? TargetSocket{get;set;} = null;
    private string Ip{get;set;} = "";
    private int Port{get;set;} = 11000;
    private bool isWork{get;set;} = true;
    public void Send(byte[] data,bool force)
    {
        switch(SendTypeObj){
            case SendType.BLOCK: SendBlock(data,force);break;
            case SendType.STREAM: SendStream(data,force);break;
        }
    }
    private void SendStream(byte[] data,bool force){
        try{
            TargetSocket?.Send(data);
        }catch{
            Disconnect();
            return;
        }
    }
    private void SendBlock(byte[] data,bool force){
        try{
            lock(BufferLocker){
                try{
                    if(!force)
                        buffer.Append(Encoding.Unicode.GetString(data));
                    if((force && buffer.Length > 0) || buffer.Length >= MaxBuffSize){
                        TargetSocket?.Send(Encoding.Unicode.GetBytes(buffer.ToString()));
                        buffer.Clear();
                    }
                }catch{
                    Disconnect();
                }
            }
        }catch{
            Disconnect();
            return;
        }
    }
    private void Connect(){
        if(isConnect())
            return;
        try{
            TargetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint TargetAddr = new IPEndPoint(IPAddress.Parse(Ip),Port);
            TargetSocket.Connect(TargetAddr);
        }
        catch{
            
        }
    }
    public bool isConnect(){
        if(TargetSocket is null)
            return false;
        return TargetSocket.Connected;
    }
    public void Exec(string ip,int port){
        this.Ip = ip;
        this.Port = port;
        while(isWork){
            if(!isConnect())
                Connect();
            else if(SendTypeObj == SendType.BLOCK){
                lock(BufferLocker){
                    Send(Encoding.Unicode.GetBytes(buffer.ToString()),true);
                }
            }
            Thread.Sleep((int)TimeToTryConnect);
        }
    }
    public void Disconnect(){
        try{
            TargetSocket?.Shutdown(SocketShutdown.Both);
            TargetSocket?.Close();
        }
        catch{

        }
        finally{
            TargetSocket = null;
        }
    }
}