using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Win32;
using System.Reflection;

namespace ImapNotify {
	class Util
	{

	    private const string RegistryPath = @"Software\Oblpro\Notify";

		/// <summary>
		/// Configures whether program is run on startup by modifying the
		/// Run settings in the registry under HKEY_CURRENT_USER.
		/// </summary>
		/// <param name="Run">Set to true to run program at startup</param>
		public static void RunOnStartup(bool Run) {
			string Path = @"Software\Microsoft\Windows\CurrentVersion\Run";
			AssemblyTitleAttribute Title = Assembly.GetExecutingAssembly().
				GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]
				as AssemblyTitleAttribute;
			RegistryKey Key = Registry.CurrentUser.OpenSubKey(Path, true);

			if (Run)
				Key.SetValue(Title.Title, Assembly.GetExecutingAssembly().Location);
			else
				Key.DeleteValue(Title.Title, false);
		}

        public static void SavePassword(string password)
        {
            RegistryKey Key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            Key.SetValue("password", password);
        }

	    public static void CheckInitRegistry()
	    {
            RegistryKey Key = Registry.CurrentUser.CreateSubKey(RegistryPath);
	        try
	        {
	            Key.GetValue("version");
	        }
	        catch (NullReferenceException)
	        {
                Key.SetValue("version", "0.1");
            }
	    }

        public static string GetPassword()
        {
            RegistryKey Key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            return (string)Key.GetValue("password");
        }

	    public static void DeletePassword()
	    {
	        RegistryKey Key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
	        try
	        {
	            Key.DeleteValue("password");
	        }
	        catch(ArgumentException)
	        {
	            //
	        }
	}

        /// <summary>
        /// Checks whether an internet connection is available.
        /// </summary>
        /// <returns>True if an internet connection is available, otherwise
        /// true is returned</returns>
        public static bool IsInternetConnected(bool fast = true) {
      try {
        /* dns.msftncsi.com is guaranteed to resolve to
         * 131.107.255.255
         */
        IPHostEntry Entry = Dns.GetHostEntry("dns.msftncsi.com");

        if (Entry.AddressList[0].ToString() != "131.107.255.255")
          return false;
        if (!fast) {
          /* fetch http://www.msftncsi.com/ncsi.txt which is
           * guaranteed to contain text string "Microsoft NCSI"
           */
          String Ncsi = (new WebClient()).DownloadString(
            "http://www.msftncsi.com/ncsi.txt");
          if (Ncsi != "Microsoft NCSI")
            return false;
        }
      } catch (Exception) {
        return false;
      }
      return true;
    }
  }
}
