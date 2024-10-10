#include "windows.h"
#include <iostream>
#include <string>
#include <sstream>
#include <memory>
using CallbackFunction = void (*)(wchar_t*,unsigned short,byte isUpper);

CallbackFunction callback = nullptr;

LRESULT CALLBACK proc(unsigned int code,WPARAM wp,LPARAM lp){
    const unsigned short str_size = 0xF;
    const unsigned short key_state_size = 0x00FF;
    PKBDLLHOOKSTRUCT hook_info = (PKBDLLHOOKSTRUCT)lp;
    switch (wp)
    {
    case WM_KEYDOWN:{
        auto IsUpper = []()->byte{
            bool isActive = GetKeyState(VK_CAPITAL) & 0x0001;
            bool isPush = GetKeyState(VK_SHIFT) & 0x8000;
            if(isActive ^ isPush)
                return (byte)true;
            return (byte)false;
        };
        std::shared_ptr<byte[]> keyboard_state(new byte[key_state_size]);
        GetKeyboardState(keyboard_state.get());
        std::shared_ptr<wchar_t[]> str(new wchar_t[str_size]);
        HKL lang = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(),NULL));
        if(ToUnicodeEx(hook_info->vkCode,hook_info->scanCode,keyboard_state.get(),str.get(),str_size,(1 << 2),lang) <= 0){
            unsigned int vkey = MapVirtualKeyExW(hook_info->vkCode,0,lang) << 16;
            if(!(hook_info->vkCode <= 1 << 31))
                vkey |= 1 << 24;
            GetKeyNameTextW(vkey,str.get(),str_size);
        }
        if(callback != nullptr)
            callback(str.get(),wcslen(str.get()),IsUpper());
    };break;
    }
    return CallNextHookEx(NULL,code,wp,lp);
}

extern "C" _stdcall void SetKeyHook(CallbackFunction func,bool& isWork){
    callback = func;
    HHOOK hook = SetWindowsHookEx(WH_KEYBOARD_LL,(HOOKPROC)proc,NULL,0);
    MSG msg = {0};
    while(GetMessage(&msg,NULL,0,0) && isWork){
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    UnhookWindowsHookEx(hook);
}