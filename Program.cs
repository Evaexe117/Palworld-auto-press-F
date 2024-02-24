using PalWorldPressF;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

class Program
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const byte VK_F = 0x46; // Virtual key code for 'F'
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static bool toggle = false;
    private static Thread sendFThread;
    private static IntPtr gameHandle = IntPtr.Zero;
    public static StatusForm statusForm;
    public static bool isApplicationRunning = true;

    [STAThread]
    public static void Main()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);

        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            try
            {
                ProcessStartInfo procInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                };

                Process.Start(procInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("This application requires administrator privileges. " + ex.Message);
                return;
            }
        }
        else
        {
            statusForm = new StatusForm();
            _hookID = SetHook(_proc);
            sendFThread = new Thread(SendFLoop);
            sendFThread.IsBackground = true;
            sendFThread.Start();
            Application.Run(statusForm);
            UnhookWindowsHookEx(_hookID);
        }
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Debug.WriteLine("Key pressed: " + (Keys)vkCode); // Debugging

            if ((Keys)vkCode == Keys.P && (Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                toggle = !toggle;
                Debug.WriteLine("CTRL+P pressed - Toggle is now " + toggle); // Debugging
                // Update status in the window
                statusForm?.Invoke(new Action(() =>
                {
                    statusForm.UpdateStatus(toggle);
                }));

                if (toggle)
                {
                    Process[] processes = Process.GetProcessesByName("Palworld-Win64-Shipping");
                    if (processes.Length > 0)
                    {
                        gameHandle = processes[0].MainWindowHandle;
                        statusForm.Invoke(new Action(() => statusForm.AppendDebugMessage("Palworld handle found: " + gameHandle)));

                    }
                    else
                    {
                        statusForm.Invoke(new Action(() => statusForm.AppendDebugMessage("PALWORLD not found.")));
                    }
                }

                return (IntPtr)1; // Prevent further processing of the key
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void SendFLoop()
    {
        while (isApplicationRunning)
        {
            if (toggle && gameHandle != IntPtr.Zero)
            {
                // Simulate a continuous press of the 'F' key
                PostMessage(gameHandle, WM_KEYDOWN, VK_F, 0);
            }
            // No action needed in the "else" case, as no key press will be sent
            Thread.Sleep(100); // Delay to prevent too rapid execution
        }
    }

    // DLLImports and function signatures follow, enabling the low-level keyboard hook, message posting, and other Windows API interactions

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out IntPtr ProcessId);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
}
