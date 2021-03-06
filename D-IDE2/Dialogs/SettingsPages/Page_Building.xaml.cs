﻿using D_IDE.Core;

namespace D_IDE.Dialogs.SettingsPages
{
	public partial class Page_Building : AbstractSettingsPage
	{
		public Page_Building()
		{
			InitializeComponent();
			LoadCurrent();
		}

		public override string SettingCategoryName
		{
			get
			{
				return "Building";
			}
		}

		public override bool ApplyChanges()
		{
			GlobalProperties.Instance.DoAutoSaveOnBuilding = cb_SaveBeforeBuild.IsChecked.Value;
			GlobalProperties.Instance.VerboseBuildOutput = cb_VerboseBuildOutput. IsChecked.Value;
			GlobalProperties.Instance.DefaultBinariesPath = tb_DefBinPath.Text;
			GlobalProperties.Instance.EnableIncrementalBuild = checkBox_IncrBuild.IsChecked.Value;
			return true;
		}

		public override void LoadCurrent()
		{
			cb_VerboseBuildOutput.IsChecked = GlobalProperties.Instance.VerboseBuildOutput;
			cb_SaveBeforeBuild.IsChecked = GlobalProperties.Instance.DoAutoSaveOnBuilding;
			tb_DefBinPath.Text = GlobalProperties.Instance.DefaultBinariesPath;
			checkBox_IncrBuild.IsChecked = GlobalProperties.Instance.EnableIncrementalBuild;
		}
	}
}
