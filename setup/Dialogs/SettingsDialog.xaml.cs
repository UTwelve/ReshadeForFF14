﻿/**
 * Copyright (C) 2014 Patrick Mours. All rights reserved.
 * License: https://github.com/crosire/reshade#license
 */

using ReShade.Utilities;
using System.IO;
using System.Windows;

namespace ReShade.Setup
{
	public partial class SettingsDialog
	{
		readonly string configFilePath;

		public SettingsDialog(string configPath)
		{
			InitializeComponent();
			configFilePath = configPath;

			OnReload(this, null);
		}

		void OnSave(object sender, RoutedEventArgs e)
		{
			var iniFile = new IniFile(configFilePath);

			iniFile.SetValue("GENERAL", "CurrentPresetPath", Preset.Text);
			iniFile.SetValue("GENERAL", "EffectSearchPaths", EffectsPath.Text);
			iniFile.SetValue("GENERAL", "TextureSearchPaths", TexturesPath.Text);
			iniFile.SetValue("GENERAL", "ScreenshotPath", ScreenshotPath.Text);

			iniFile.SetValue("GENERAL", "PerformanceMode", CheckboxValue(PerformanceMode.IsChecked));
			iniFile.SetValue("GENERAL", "ShowFPS", CheckboxValue(ShowFps.IsChecked));
			iniFile.SetValue("GENERAL", "ShowClock", CheckboxValue(ShowClock.IsChecked));

			var skipTut = SkipTut.IsChecked;
			iniFile.SetValue("GENERAL", "TutorialProgress", skipTut.HasValue ? (skipTut.Value ? "4" : "0") : iniFile.GetString("GENERAL", "TutorialProgress", "0"));

			iniFile.Save();

			DialogResult = true;
		}

		void OnCancel(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		void OnReload(object sender, RoutedEventArgs e)
		{
			if (!File.Exists(configFilePath))
				return;

			var iniFile = new IniFile(configFilePath);

			Preset.Text = iniFile.GetString("GENERAL", "CurrentPresetPath");
			EffectsPath.Text = iniFile.GetString("GENERAL", "EffectSearchPaths");
			TexturesPath.Text = iniFile.GetString("GENERAL", "TextureSearchPaths");
			ScreenshotPath.Text = iniFile.GetString("GENERAL", "ScreenshotPath");

			PerformanceMode.IsChecked = iniFile.GetString("GENERAL", "PerformanceMode") == "1";
			ShowFps.IsChecked = iniFile.GetString("GENERAL", "ShowFPS") == "1";
			ShowClock.IsChecked = iniFile.GetString("GENERAL", "ShowClock") == "1";

			string tutProgress = iniFile.GetString("GENERAL", "TutorialProgress", "0");
			if (tutProgress == "0" || tutProgress == "4")
			{
				SkipTut.IsThreeState = false;
				SkipTut.IsChecked = tutProgress == "4";
				SkipTut.ToolTip = null;
			}
			else
			{
				SkipTut.IsThreeState = true;
				SkipTut.IsChecked = null;
				SkipTut.ToolTip = "Keep existing tutorial progress";
			}
		}

		void OnChoosePresetDialog(object sender, RoutedEventArgs e)
		{
			string origFirstValue = (Preset.Text ?? string.Empty).Split(',')[0];

			var dlg = new FileOpenDialog
			{
				CheckPathExists = true,
				CheckFileExists = false,
				Filter = "Preset Files (*.ini, *.txt)|*.ini;*.txt",
				FileName = origFirstValue,
				DefaultExt = ".ini",
				InitialDirectory = Path.GetDirectoryName(origFirstValue),
			};

			if (dlg.ShowDialog(this) == true)
			{
				Preset.Text = dlg.FileName;
			}
		}

		void OnChooseFolderDialog(object sender, RoutedEventArgs e)
		{
			var target = e.Source as FrameworkElement;
			string origFirstValue = (target.Tag as string ?? string.Empty).Split(',')[0].TrimEnd(Path.PathSeparator);

			var dlg = new FileOpenDialog
			{
				Multiselect = true,
				FolderPicker = true,
				CheckPathExists = true,
				InitialDirectory = origFirstValue
			};

			if (dlg.ShowDialog(this) == true)
			{
				target.Tag = string.Join(",", dlg.FileNames);
			}
		}

		private static string CheckboxValue(bool? check) => (check.HasValue && check.Value ? "1" : "0");
	}
}
