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
    private static IntPtr notepadHandle = IntPtr.Zero;
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
                MessageBox.Show("Cette application a besoin de privilèges d'administrateur. " + ex.Message);
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
            Debug.WriteLine("Touche pressée: " + (Keys)vkCode); // Débogage

            if ((Keys)vkCode == Keys.P && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                toggle = !toggle;
                Debug.WriteLine("Shift+P pressé - Toggle est maintenant " + toggle); // Débogage
                //Affichage du statu dans la fentre
                statusForm?.Invoke(new Action(() =>
                {
                    statusForm.UpdateStatus(toggle);
                }));

                if (toggle)
                {
                    Process[] processes = Process.GetProcessesByName("Palworld-Win64-Shipping");
                    if (processes.Length > 0)
                    {
                        notepadHandle = processes[0].MainWindowHandle;
                        Debug.WriteLine("Handle de Palworld trouvé: " + notepadHandle); // Débogage
                    }
                    else
                    {
                        Debug.WriteLine("PALWORLD n'est pas trouvé."); // Débogage
                    }
                }

                return (IntPtr)1; // Empêcher le traitement ultérieur de la touche
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    private static bool isKeyDownSent = false;

    private static void SendFLoop()
    {
        while (isApplicationRunning)
        {
            if (toggle && notepadHandle != IntPtr.Zero)
            {
                // Simuler un appui continu sur la touche 'F'
                PostMessage(notepadHandle, WM_KEYDOWN, VK_F, 0);
            }
            // Pas besoin de faire quoi que ce soit dans le "else", car aucune touche ne sera envoyée
            Thread.Sleep(100); // Délai pour éviter une exécution trop rapide
        }
    }






    [DllImport("user32.dll", SetLastError = true)]
    static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

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
