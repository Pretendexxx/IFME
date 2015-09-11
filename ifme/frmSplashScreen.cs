﻿using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

using static System.Console;

enum DownloadType
{
	Plugin,
	Extension
}

namespace ifme
{
    public partial class frmSplashScreen : Form
	{
		WebClient client = new WebClient();
		bool finish = false;

		Stopwatch stopwatch;
		long ctime = 0, ptime = 0, current = 0, previous = 0;

		public frmSplashScreen()
		{
			InitializeComponent();
			Icon = Properties.Resources.ifme5;
			BackgroundImage = Global.GetRandom % 2 != 0 ? Properties.Resources.SplashScreen6A : Properties.Resources.SplashScreen6B;

			client.DownloadProgressChanged += client_DownloadProgressChanged;
			client.DownloadFileCompleted += client_DownloadFileCompleted;
		}

		private void frmSplashScreen_Load(object sender, EventArgs e)
		{
			Title = "Nemu Bootstrap";
			WriteLine(@"_____   __                      ___            ________            _____ ");
			WriteLine(@"___  | / /___________ _______  __( )_______    ___  __ )_____________  /_");
			WriteLine(@"__   |/ /_  _ \_  __ `__ \  / / /|/__  ___/    __  __  |  __ \  __ \  __/");
			WriteLine(@"_  /|  / /  __/  / / / / / /_/ /   _(__  )     _  /_/ // /_/ / /_/ / /_  ");
			WriteLine(@"/_/ |_/  \___//_/ /_/ /_/\__,_/    /____/      /_____/ \____/\____/\__/  ");
			WriteLine();
			WriteLine(@"                                                           nemuserver.net");
			WriteLine();

			bgwThread.RunWorkerAsync();
		}

		private void bgwThread_DoWork(object sender, DoWorkEventArgs e)
		{
			// Upgrade settings
			if (!string.Equals(Properties.Settings.Default.Version, Global.App.VersionRelease))
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.Version = Global.App.VersionRelease;
				Properties.Settings.Default.Save();

				WriteLine("Settings has been upgraded!");
			}

			// Language
			if (!File.Exists(Path.Combine(Global.Folder.Language, $"{Properties.Settings.Default.Language}.ini")))
			{
				Properties.Settings.Default.Language = "en";
				WriteLine($"Language file {Properties.Settings.Default.Language}.ini not found, make sure file name and CODE are same");
            }
			else
			{
				Language.Display();
				WriteLine("Loading language file");
			}

			// CPU Affinity, Load previous, if none, set default all CPU
			if (string.IsNullOrEmpty(Properties.Settings.Default.CPUAffinity))
			{
				Properties.Settings.Default.CPUAffinity = TaskManager.CPU.DefaultAll(true);
				Properties.Settings.Default.Save();

				WriteLine("Applying CPU settings...");
			}

			string[] aff = Properties.Settings.Default.CPUAffinity.Split(',');
			for (int i = 0; i < Environment.ProcessorCount; i++)
			{
				TaskManager.CPU.Affinity[i] = Convert.ToBoolean(aff[i]);
			}

			// Detect AviSynth
			if (File.Exists(Plugin.AviSynthFile))
			{
				Plugin.AviSynthInstalled = true;
				WriteLine("AviSynth detected!");
			}
			else
			{
				Plugin.AviSynthInstalled = false;
				WriteLine("AviSynth not detected!");
			}

			// Thanks to our donor
			try
			{
				WriteLine("Loading our donor list :) you can see via \"About IFME\"");
				File.WriteAllText("metauser.if", client.DownloadString("http://x265.github.io/supporter.txt"), Encoding.UTF8);
			}
			catch (Exception)
			{
				Write("Sorry, cannot load something :( it seem no Internet");
			}

			// Setting Load
			SettingLoad();

			// Plugin 
			PluginCheck(); // check repo
			Plugin.Load(); // load to memory
			PluginUpdate(); // apply update
			Plugin.Load(); // reload

			// Check x265 compiler binary
			if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "x265icc")))
				Properties.Settings.Default.Compiler = "gcc";
			else
				Plugin.IsExistHEVCICC = true;

			if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "x265msvc")))
				Properties.Settings.Default.Compiler = "gcc";
			else
				Plugin.IsExistHEVCMSVC = true;

			// Profile
			Profile.Load();

			// Extension
			Extension.Load();
			Extension.CheckDefault();
			ExtensionUpdate();
			Extension.Load(); // reload

			// App Version
#if NONSTEAM
			if (!string.Equals(Global.App.VersionRelease, client.DownloadString("https://x265.github.io/update/version_ifme5.txt")))
				Global.App.NewRelease = true;
#endif
			
			// For fun
			WriteLine("\n\nAll done! Initialising...");
			System.Threading.Thread.Sleep(3000);

			// Save all settings
			Properties.Settings.Default.Save();
		}

		private void bgwThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Close();
		}

		#region Settings
		void SettingLoad()
		{
			// Settings
			if (string.IsNullOrEmpty(Properties.Settings.Default.DirOutput))
				Properties.Settings.Default.DirOutput = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "IFME");

			if (!Directory.Exists(Properties.Settings.Default.DirOutput))
				Directory.CreateDirectory(Properties.Settings.Default.DirOutput);

			if (string.IsNullOrEmpty(Properties.Settings.Default.DirTemp))
				Properties.Settings.Default.DirTemp = Global.Folder.Temp;

			if (!Directory.Exists(Properties.Settings.Default.DirTemp))
				Directory.CreateDirectory(Properties.Settings.Default.DirTemp);
		}
		#endregion

		#region Plugins
		public void PluginCheck()
		{
			// Check folder
			if (OS.IsWindows)
			{
				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "ffmpeg")))
				{
					WriteLine("Downloading component  1 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/ffmpeg.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "avisynth")))
				{
					WriteLine("Downloading component  2 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/avisynth.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "ffmsindex")))
				{
					WriteLine("Downloading component  3 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/ffmsindex.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "mp4fpsmod")))
				{
					WriteLine("Downloading component  4 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/mp4fpsmod.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "mkvtool")))
				{
					WriteLine("Downloading component  5 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/mkvtool.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "mp4box")))
				{
					WriteLine("Downloading component  6 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/mp4box.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "x265gcc")))
				{
					WriteLine("Downloading component  7 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/x265gcc.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "x265icc")))
				{
					WriteLine("Downloading component  8 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/x265icc.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "x265msvc")))
				{
					WriteLine("Downloading component  9 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/x265msvc.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "flac")))
				{
					WriteLine("Downloading component 10 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/flac.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "ogg")))
				{
					WriteLine("Downloading component 11 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/ogg.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "opus")))
				{
					WriteLine("Downloading component 12 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/opus.ifx");
				}

				if (!Directory.Exists(Path.Combine(Global.Folder.Plugins, "faac")))
				{
					WriteLine("Downloading component 13 of 13");
					Download("http://master.dl.sourceforge.net/project/ifme/plugins/ifme5/windows/faac.ifx");
				}
			}
			else
			{
				// coming soon
			}
		}

		void PluginUpdate()
		{
			foreach (var item in Plugin.List)
			{
				Write($"\nChecking for update: {item.Profile.Name}");

				if (string.IsNullOrEmpty(item.Provider.Update))
					continue;

				if (string.Equals(item.Profile.Ver, client.DownloadString(item.Provider.Update)))
					continue;

				Download(item.Provider.Download, Global.Folder.Plugins, "update.ifx");
			}
		}
		#endregion

		void ExtensionUpdate()
		{
			foreach (var item in Extension.Items)
			{
				Write($"\nChecking for update: {item.Name}");

				if (string.IsNullOrEmpty(item.UrlVersion))
					continue;

				string version = client.DownloadString(item.UrlVersion);

				if (string.Equals(item.Version, version))
					continue;

				string link = string.Format(item.UrlDownload, version);

				Download(link, Global.Folder.Extension, "zombie.ife");
			}
		}

		void Download(string url)
		{
			Download(url, Global.Folder.Plugins, "imouto.ifx");
		}

		void Download(string url, string folder, string file)
		{
			Write("\n");

			try
			{
				finish = false;

				stopwatch = new Stopwatch();
				client.DownloadFileAsync(new Uri(url), Path.Combine(folder, file));
				stopwatch.Start();

				while (finish == false) { /* doing nothing, just block */ }

				Extract(folder, file);
			}
			catch
			{
				WriteLine("File not found or Offline");
			}
		}

		void Extract(string dir, string file)
		{
			string unzip = Path.Combine(Global.Folder.AppDir, "7za");
			string zipfile = Path.Combine(dir, file);

			Write("\nExtracting... ");
			TaskManager.Run($"\"{unzip}\" x \"{zipfile}\" -y \"-o{dir}\" > {OS.Null} 2>&1");
			
			Write("Done!\n");
			File.Delete(zipfile);
		}

		void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			// Info: http://www.doyatte.com/how-to-get-the-download-speed-while-using-downloaddataasync/
			// get the elapsed time in milliseconds
			ctime = stopwatch.ElapsedMilliseconds;
			// get the received bytes at the particular instant
			current = e.BytesReceived;
			// calculate the speed the bytes were downloaded and assign it to a Textlabel (speedLabel in this instance)
			int speed = ((int)(((current - previous) / (double)1024) / ((ctime - ptime) / (double)1000)));

			previous = current;
			ptime = ctime;

			Write($"\r{((float)e.BytesReceived / e.TotalBytesToReceive):P} Completed... ({(speed > 0 ? speed : 0)} KB/s)\t");
		}

		void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			finish = true;
		}
	}
}
