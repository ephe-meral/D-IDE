﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace D_IDE.Core
{
	/// <summary>
	/// Since the WPF Designer of Visual Studio enforces declarable classes, there is a faked abstractness
	/// </summary>
	public class AbstractSettingsPage : UserControl
	{
		public virtual void ApplyChanges(){}
		public virtual void RestoreDefaults() { }
		public virtual void LoadCurrent() { }

		public virtual string SettingCategoryName { get { return String.Empty; } }
		public virtual AbstractSettingsPage[] SubCategories { get { return null; } }
	}

	public class AbstractProjectSettingsPage :UserControl
	{
		public virtual void ApplyChanges(Project Project) { }
		public virtual void LoadCurrent(Project Project) { }

		public virtual string SettingCategoryName { get { return String.Empty; } }
	}
}