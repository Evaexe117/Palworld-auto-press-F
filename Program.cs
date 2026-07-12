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
    private const int WM_SYSKEYDOWN = 0x0104; // key down while ALT is held
    private const byte VK_F = 0x46; // Virtual key code for 'F'
    private const int LPARAM_AUTOREPEAT = 0x40000000; // bit 30: previous key state was down
    private const int LPARAM_KEYUP = unchecked((int)0xC0000001); // repeat count 1 + bits 30/31 set
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static bool toggle = false;
    private static bool holdToggle = false;
    public static volatile int spamIntervalMs = 100;
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
            ReleaseHoldIfActive();
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
        // ALT combos arrive as WM_SYSKEYDOWN, not WM_KEYDOWN
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Debug.WriteLine("Key pressed: " + (Keys)vkCode); // Debugging

            if ((Keys)vkCode == Keys.J && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                toggle = !toggle;
                Debug.WriteLine("ALT+J pressed - Spam toggle is now " + toggle); // Debugging

                if (toggle)
                {
                    ReleaseHoldIfActive(); // spam and hold modes are exclusive
                    FindGameHandle();
                }

                UpdateStatusDisplay();
                return (IntPtr)1; // Prevent further processing of the key
            }

            if ((Keys)vkCode == Keys.H && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                if (holdToggle)
                {
                    ReleaseHoldIfActive();
                }
                else
                {
                    toggle = false; // spam and hold modes are exclusive
                    FindGameHandle();
                    if (gameHandle != IntPtr.Zero)
                    {
                        holdToggle = true;
                        PostMessage(gameHandle, WM_KEYDOWN, VK_F, 0);
                    }
                }
                Debug.WriteLine("ALT+H pressed - Hold toggle is now " + holdToggle); // Debugging

                UpdateStatusDisplay();
                return (IntPtr)1; // Prevent further processing of the key
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void FindGameHandle()
    {
        Process[] processes = Process.GetProcessesByName("Palworld-Win64-Shipping");
        if (processes.Length > 0)
        {
            gameHandle = processes[0].MainWindowHandle;
            statusForm?.Invoke(new Action(() => statusForm.AppendDebugMessage("Palworld handle found: " + gameHandle)));
        }
        else
        {
            gameHandle = IntPtr.Zero;
            statusForm?.Invoke(new Action(() => statusForm.AppendDebugMessage("PALWORLD not found.")));
        }
    }

    public static void ReleaseHoldIfActive()
    {
        if (holdToggle)
        {
            holdToggle = false;
            if (gameHandle != IntPtr.Zero)
            {
                PostMessage(gameHandle, WM_KEYUP, VK_F, LPARAM_KEYUP);
            }
        }
    }

    private static void UpdateStatusDisplay()
    {
        statusForm?.Invoke(new Action(() =>
        {
            statusForm.UpdateStatus(toggle, holdToggle);
        }));
    }

    private static void SendFLoop()
    {
        while (isApplicationRunning)
        {
            if (gameHandle != IntPtr.Zero)
            {
                if (toggle)
                {
                    // Spam mode: simulate discrete presses of the 'F' key
                    PostMessage(gameHandle, WM_KEYDOWN, VK_F, 0);
                }
                else if (holdToggle)
                {
                    // Hold mode: simulate keyboard auto-repeat while the key stays down
                    PostMessage(gameHandle, WM_KEYDOWN, VK_F, LPARAM_AUTOREPEAT);
                }
            }
            Thread.Sleep(toggle ? Math.Max(10, spamIntervalMs) : 100);
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
