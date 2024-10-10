using System.Net;
using System.Net.Sockets;
using System.Text;
class TcpServer{
    private Socket? listener{get;set;} = null;
    private IPEndPoint? SockInfo{get;set;} = null;
    private Dictionary<string,Socket> AllConnection{get;set;} = new Dictionary<string,Socket>();
    private bool IsExec{get;set;} = false;
    public bool IsServerWorking{get;private set;} = false;
    private ReadSlot? ReadProc = null;
    public SettingsServer settings{get;set;} = new SettingsServer();
    private void Read(object? sock){
        if(sock is null)
            return;
        Socket ClientSocket = (Socket)sock;
        string? ip = null;
        byte[] data = new byte[255];
        int length = 0;
        try{
            ip = ((IPEndPoint?)ClientSocket.RemoteEndPoint)?.Address.ToString();
            while(ClientSocket.Connected){
                length = ClientSocket.Receive(data);
                ReadProc?.Invoke(data,length,ClientSocket);
            }
        }
        catch{
            
        }
        finally{
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            if(ip is not null)
                AllConnection.Remove(ip);
        }
    }
    public void Send(string IpAdress, byte[] data){
        if(!AllConnection.ContainsKey(IpAdress))
            return;
        Socket? ClilentSocket = AllConnection[IpAdress];
        if(!ClilentSocket.Connected)
            return;
        try{
            ClilentSocket.Send(data);
        }catch{
            return;
        }
    }
    public void ExecServer(SettingsServer settings,ReadSlot ReadProc){
        if(IsServerWorking || IsExec)
            return;
        this.settings = settings;
        IsExec = true;
        this.ReadProc = ReadProc;
        IPAddress ip = IPAddress.Parse(this.settings.Ip);
        this.SockInfo = new IPEndPoint(ip,this.settings.Port);
        listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        listener.Bind(this.SockInfo);
        listener.Listen(this.settings.MaxConnection);
        Thread ThreadWork = new Thread(delegate(){
            IsServerWorking = true;
            try{
                while(IsExec){
                    Socket NewSock =  listener.Accept();
                    IPAddress? RemoteIp =  ((IPEndPoint?)NewSock.RemoteEndPoint)?.Address;
                    if(RemoteIp is null)
                        AllConnection.Add("Empty",NewSock);
                    else
                        AllConnection[RemoteIp.ToString()] = NewSock;
                    ThreadPool.QueueUserWorkItem(Read,NewSock);
                }
            }catch(SocketException exep){
                if(IsExec){
                    File.AppendAllText(this.settings.PathToErrorLog,exep.Message + Environment.NewLine);
                }
            }catch(Exception exep){
                File.AppendAllText(this.settings.PathToErrorLog,exep.Message + Environment.NewLine);
            }
            finally{
                IsServerWorking = false;
            }
        });
        ThreadWork.Start();
    }
    public void CloseServer(){
        this.IsExec = false;
        foreach(var pair in AllConnection){
            pair.Value.Shutdown(SocketShutdown.Both);
            pair.Value.Close();
        }
        AllConnection.Clear();
        listener?.Close();
    }
    public delegate void ReadSlot(byte[] data,int length,Socket ClientSocket);
}