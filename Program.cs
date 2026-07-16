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
    private const int LPARAM_KEYUP = unchecked((int)0xC0000001); // repeat count 1 + bits 30/31 set
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static bool toggle = false;
    private static bool? hadFocus = null; // last known focus state of the game window while active
    public static volatile int spamIntervalMs = 100;
    public static volatile int pressedKeyCode = (int)Keys.F;
    public static volatile int toggleKeyCode = (int)Keys.J;
    private static Thread sendKeyThread;
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
            sendKeyThread = new Thread(SendKeyLoop);
            sendKeyThread.IsBackground = true;
            sendKeyThread.Start();
            Application.Run(statusForm);
            if (toggle)
            {
                ReleasePressedKey();
            }
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

            if (vkCode == toggleKeyCode && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                toggle = !toggle;
                Debug.WriteLine($"ALT+{(Keys)toggleKeyCode} pressed - Spam toggle is now {toggle}"); // Debugging

                if (toggle)
                {
                    hadFocus = null;
                    FindGameHandle();
                }
                else
                {
                    ReleasePressedKey();
                }

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

    public static void SetPressedKey(Keys key)
    {
        int previousKeyCode = pressedKeyCode;
        if (previousKeyCode == (int)key)
        {
            return;
        }

        // Swap the key code before releasing the old one, otherwise the send
        // loop can post one more key-down of the old key after its release,
        // leaving it stuck held in the game
        pressedKeyCode = (int)key;

        if (toggle)
        {
            ReleaseKey(previousKeyCode);
            hadFocus = null;
        }
    }

    public static void SetToggleKey(Keys key)
    {
        toggleKeyCode = (int)key;
    }

    private static void ReleasePressedKey()
    {
        ReleaseKey(pressedKeyCode);
        hadFocus = null;
    }

    private static void ReleaseKey(int keyCode)
    {
        // The auto-press never sends key-ups, so tell the game the configured
        // key is released when the toggle turns off (or the app exits) to avoid
        // a stuck key.
        if (gameHandle != IntPtr.Zero)
        {
            PostMessage(gameHandle, WM_KEYUP, keyCode, LPARAM_KEYUP);
        }
    }

    private static void UpdateStatusDisplay()
    {
        statusForm?.Invoke(new Action(() =>
        {
            statusForm.UpdateStatus(toggle);
        }));
    }

    private static void SendKeyLoop()
    {
        while (isApplicationRunning)
        {
            if (gameHandle != IntPtr.Zero && toggle)
            {
                // The game flushes its pressed-key state whenever the window gains or
                // loses focus (UE FlushPressedKeys), which kills the held key: on every
                // focus transition, send an explicit release so the next key-down is
                // seen as a fresh press re-establishing the hold
                bool gameHasFocus = GetForegroundWindow() == gameHandle;
                if (hadFocus.HasValue && hadFocus.Value != gameHasFocus)
                {
                    PostMessage(gameHandle, WM_KEYUP, pressedKeyCode, LPARAM_KEYUP);
                    Thread.Sleep(60); // games may merge a release + re-press within ~50 ms
                    statusForm?.Invoke(new Action(() => statusForm.AppendDebugMessage("Focus changed - hold re-primed")));
                }
                hadFocus = gameHasFocus;

                // Simulate discrete presses of the configured key. No key-up is sent between
                // presses, so the game also sees the key as continuously held, which covers
                // hold interactions too (when "Single button press for hold
                // interactions" is disabled in Palworld's options)
                PostMessage(gameHandle, WM_KEYDOWN, pressedKeyCode, 0);
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

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
}
