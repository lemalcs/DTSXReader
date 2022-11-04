using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace DTSXExplorer
{
    /// <summary>
    /// Interaction logic for SQLServerConnectionWindow.xaml
    /// </summary>
    public partial class SQLServerConnectionWindow : Window, IPasswordContainer
    {
        #region Native methods and constants
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        private static extern IntPtr DestroyMenu(IntPtr hWnd);

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;
        private const uint SC_CLOSE = 0xF060;
        IntPtr menuHandle;

        private IntPtr windowHandle;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        public SQLServerConnectionWindow()
        {
            InitializeComponent();
            this.SourceInitialized += SQLServerConnectionWindow_SourceInitialized;
        }

        private void SQLServerConnectionWindow_SourceInitialized(object sender, EventArgs e)
        {
            windowHandle = new WindowInteropHelper(this).Handle;
            if (windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            menuHandle = GetSystemMenu(windowHandle, false);
            if (menuHandle != IntPtr.Zero)
            {
                // Disable close button
                EnableMenuItem(menuHandle, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);

                // Hide close button
                SetWindowLong(windowHandle, GWL_STYLE, GetWindowLong(windowHandle, GWL_STYLE) & ~WS_SYSMENU);
            }
        }

        /// <summary>
        /// The password to authenticate user against a SQL Server instance.
        /// </summary>
        public SecureString password => txt_Password.SecurePassword;

        /// <summary>
        /// Clears password field.
        /// </summary>
        public void Clear()
        {
            txt_Password.Clear();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void combo_Authetication_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Clear();
        }
    }
}
