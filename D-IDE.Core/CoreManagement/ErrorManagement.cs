﻿using System.Collections.Generic;
using System.Linq;

namespace D_IDE.Core
{
	public abstract partial class CoreManager
	{
		public class ErrorManagement
		{
			/// <summary>
			/// In this array, all errors are listed
			/// </summary>
			public static GenericError[] Errors { get; protected set; }

			/// <summary>
			/// Returns file specific errors.
			/// </summary>
			public static GenericError[] GetErrorsForFile(string file)
			{
				if (Errors == null || Errors.Length < 1)
					return new GenericError[] { };

				return Errors.Where(err => err.FileName == file).ToArray();
			}

			public static GenericError[] LastParseErrors
			{
				get
				{
					var ed = Instance.CurrentEditor as EditorDocument;
					IEnumerable<GenericError> errs = null;
					if (ed == null || (errs= ed.ParserErrors) == null)
						return new GenericError[] { };

					return errs.ToArray();
				}
			}

			public static readonly List<GenericError> UnboundErrors = new List<GenericError>();

			/// <summary>
			/// Refreshes the commonly used error list.
			/// Also updates the error panel's error list view.
			/// </summary>
			public static void RefreshErrorList()
			{
				var el = new List<GenericError>();

				// Add unbound build errors
				if (UnboundErrors.Count>0)
					el.AddRange(UnboundErrors);
				// (Bound) Solution errors
				else if (CurrentSolution != null)
					foreach (var prj in CurrentSolution)
					{
						// Add project errors
						if (prj.LastBuildResult != null)
							el.AddRange(prj.LastBuildResult.BuildErrors);
						// Add errors of its modules
						foreach (var m in prj.Files)
							if(m.LastBuildResult!=null)
								el.AddRange(m.LastBuildResult.BuildErrors);
					}
				
				// Errors that occurred while parsing source files
				var curEd=CoreManager.Instance.CurrentEditor as IEditorDocument;
				if (curEd!=null)
				{
					// If current module is unbound, show its errors exclusively
					var errs = curEd.ParserErrors;
					if (!curEd.HasProject && errs != null)
						el.AddRange(errs);
					else
						foreach (var ed in CoreManager.Instance.Editors)
							if (ed is IEditorDocument && ed.HasProject && (errs= (ed as IEditorDocument).ParserErrors) != null)
								el.AddRange(errs);
				}

				Errors = el.ToArray();

				Instance.MainWindow.RefreshErrorList();

				foreach (var ed in CoreManager.Instance.Editors)
					if (ed is EditorDocument)
						(ed as EditorDocument).RefreshErrorHighlightings();
			}
		}

	}
}
