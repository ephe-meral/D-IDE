﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D_IDE.Core;
using System.IO;
using Microsoft.Win32;
using System.Windows;

namespace D_IDE
{
	partial class IDEManager
	{
		public class ProjectManagement
		{
			/// <summary>
			/// Creates a new project.
			/// Doesn't add it to the current solution.
			/// Doesn't modify the current solution.
			/// </summary>
			public static Project CreateNewProject(AbstractLanguageBinding Binding, FileTemplate ProjectType, string Name, string BaseDir)
			{
				/*
				 * Enforce the creation of a new project directory
				 */
				var baseDir = BaseDir.Trim('\\', ' ', '\t') + "\\" + Util.PurifyDirName(Name);
				if (Directory.Exists(baseDir))
				{
					MessageBox.Show("Project directory "+baseDir+" exists already","Project directory creation error");
					return null;
				}

				Util.CreateDirectoryRecursively(baseDir);

				var prj = Binding.CreateEmptyProject(Name, baseDir + "\\" +
					Path.ChangeExtension(Util.PurifyFileName(Name), ProjectType.Extensions[0]), ProjectType);
				return prj;
			}

			/// <summary>
			/// Note: The current solution will not be touched
			/// </summary>
			public static Solution CreateNewProjectAndSolution(AbstractLanguageBinding Binding, FileTemplate ProjectType, string Name, string BaseDir, string SolutionName)
			{
				var baseDir = BaseDir.Trim('\\', ' ', '\t');
				var sln = new Solution();
				sln.Name = SolutionName;
				sln.FileName =
					baseDir + "\\" +
					Path.ChangeExtension(Util.PurifyFileName(Name), Solution.SolutionExtension);

				AddNewProjectToSolution(sln, Binding, ProjectType, Name, baseDir);

				return sln;
			}

			/// <summary>
			/// Creates a new project and adds it to the current solution
			/// </summary>
			public static Project AddNewProjectToSolution(Solution sln, AbstractLanguageBinding Binding, FileTemplate ProjectType, string Name, string BaseDir)
			{
				var prj = CreateNewProject(Binding, ProjectType, Name, BaseDir);
				if (prj != null)
				{
					prj.Save();
					sln.AddProject(prj);
					sln.Save();
				}

				Instance.UpdateGUI();
				return prj;
			}

			/// <summary>
			/// Adds a new project to the current solution
			/// </summary>
			public static Project AddNewProjectToSolution(AbstractLanguageBinding Binding, FileTemplate ProjectType, string Name, string BaseDir)
			{
				return AddNewProjectToSolution(CurrentSolution, Binding, ProjectType, Name, BaseDir);
			}

			public static bool AddExistingProjectToSolution(Solution sln, string Projectfile)
			{
				/*
				 * a) Check if project already existing
				 * b) Add to solution; if succeeded:
				 * c) Try to load project; if succeeded:
				 * d) Save solution
				 */

				// a)
				if (sln.ContainsProject(Projectfile))
				{
					MessageBox.Show("Project" + sln[Projectfile].Name + " is already part of solution "+sln.Name, "Project addition error");
					return false;
				}
				// b)
				if (!sln.AddProject(Projectfile))
					return false;
				// c)
				var prj = Project.LoadProjectFromFile(sln, Projectfile);
				if (prj == null) return false; // Perhaps it's a project format that's simply not supported

				// d)
				sln.Save();

				Instance.UpdateGUI();
				return true;
			}

			/// <summary>
			/// Opens a dialog which asks the user to select one or more project files
			/// </summary>
			/// <returns></returns>
			public static bool AddExistingProjectToSolution(Solution sln)
			{
				var of = new OpenFileDialog();
				of.InitialDirectory = sln.BaseDirectory;

				// Build filter string
				string tfilter = "";
				var all_exts = new List<string>();
				foreach (var lang in from l in LanguageLoader.Bindings where l.ProjectsSupported select l)
				{
					tfilter += "|" + lang.LanguageName + " projects|";
					var exts = new List<string>();

					foreach (var t in lang.ProjectTemplates)
						if (t.Extensions != null)
							foreach (var ext in t.Extensions)
							{
								if (!exts.Contains("*" + ext))
									exts.Add("*" + ext);
								if (!all_exts.Contains("*" + ext))
									all_exts.Add("*" + ext);
							}

					tfilter += string.Join(";", exts);
				}
				tfilter = "All supported projects|" + string.Join(";", all_exts) + tfilter + "|All files|*.*";
				of.Filter = tfilter;

				of.Multiselect = true;

				var r = true;
				if (of.ShowDialog().Value)
				{
					foreach (var file in of.FileNames)
						if (!AddExistingProjectToSolution(sln, file))
							r = false;
				}

				return r;
			}

			public static void ReassignProject(Project Project, Solution NewSolution)
			{

			}

			public static bool Rename(Solution sln, string NewName)
			{
				// Prevent moving the project into an other directory
				if (String.IsNullOrEmpty(NewName) || NewName.Contains('\\'))
					return false;

				/*
				 * - Try to rename the solution file
				 * - Rename the solution
				 * - Save it
				 */

				var newSolutionFileName = Path.ChangeExtension(Util.PurifyFileName(NewName), Solution.SolutionExtension);
				var ret = Util.MoveFile(sln.FileName, newSolutionFileName);
				if (ret)
				{
					sln.Name = NewName;
					sln.FileName = sln.BaseDirectory + "\\" + newSolutionFileName;
					Instance.MainWindow.RefreshTitle();
					sln.Save();
				}
				return ret;
			}

			public static bool Rename(Project prj, string NewName)
			{
				// Prevent moving the project into an other directory
				if (String.IsNullOrEmpty(NewName) || NewName.Contains('\\'))
					return false;

				/*
				 * - Try to rename the project file
				 * - If successful, remove old project file from solution
				 * - Rename the project and it's filename
				 * - Add the 'new' project to the solution
				 * - Save everything
				 */

				var newSolutionFileName = Util.PurifyFileName(NewName) + Path.GetExtension(prj.FileName);
				var ret = Util.MoveFile(prj.FileName, newSolutionFileName);
				if (ret)
				{
					prj.Solution.ExcludeProject(prj.FileName);
					prj.Name = NewName;
					prj.FileName = prj.BaseDirectory + "\\" + newSolutionFileName;
					prj.Solution.AddProject(prj);

					prj.Solution.Save();
					prj.Save();
				}
				return ret;
			}

			/// <summary>
			/// (Since we don't want to remove a whole project we still can exclude them from solutions)
			/// </summary>
			/// <param name="prj"></param>
			public static void ExcludeProject(Project prj)
			{
				/*
				 * - Close open editors that are related to prj
				 * - Remove reference from solution
				 * - Save solution
				 */

				foreach (var ed in Instance.Editors.Where(e => e.Project == prj))
					ed.Close();

				var sln = prj.Solution;
				sln.ExcludeProject(prj.FileName);
				sln.Save();

				Instance.UpdateGUI();
			}

			/// <summary>
			/// (Since we don't want to remove a whole project we still can exclude them from solutions)
			/// </summary>
			public static void ExcludeProject(Solution sln, string prjFile)
			{
				/*
				 * - Remove reference from solution
				 * - Save solution
				 */

				sln.ExcludeProject(prjFile);
				sln.Save();

				Instance.UpdateGUI();
			}

			#region Project Dependencies dialog
			static void ShowProjectDependenciesDialog(Solution sln, Project Project)
			{

			}

			public static void ShowProjectDependenciesDialog(Project Project)
			{
				ShowProjectDependenciesDialog(Project.Solution, Project);
			}

			public static void ShowProjectDependenciesDialog(Solution sln)
			{
				ShowProjectDependenciesDialog(sln, null);
			}

			public static void ShowProjectPropertiesDialog(Project Project)
			{
				if (Project == null)
					return;

				var dlg = new D_IDE.Dialogs.ProjectSettingsDlg(Project);
				dlg.Owner = IDEManager.Instance.MainWindow;
				dlg.ShowDialog();
			}
			#endregion
		}
	}
}
