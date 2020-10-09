using System;
using System.Runtime.InteropServices;

namespace TaskManagement.BIZ
{
    public class ConsoleHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
    /// Imposta visibilità applicazione corrente
    /// </summary>
    /// <param name="show"></param>
    /// <remarks></remarks>
        public static void SetVisible(string title, bool show)
        {
            // Ricerca handle finestra console
            var hWnd = FindWindow(null, title);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            // Imposta visualizzazione
            ShowWindow(hWnd, Convert.ToInt32(show));
        }
    }
}