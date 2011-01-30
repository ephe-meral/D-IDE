﻿using System;
using System.IO;
using System.Linq;
using D_IDE.Core;
using D_IDE.Core.Controls.Editor;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Parser.Core;

namespace D_IDE
{
	public class EditorDocument:AbstractEditorDocument
	{
		#region Generic properties
		public readonly TextEditor Editor = new TextEditor();
		public AbstractSyntaxTree SyntaxTree=null;

		TextMarkerService MarkerStrategy;

		public bool ReadOnly
		{
			get { return Editor.IsReadOnly; }
			set { Editor.IsReadOnly = true; }
		}
		#endregion

		public EditorDocument()
		{
			Init();
		}

		public EditorDocument(string file)
			: base(file)
		{
			Init();
		}

		#region Abstract Editor method overloads
		public override bool Save()
		{
			/*
			 * If the file is still undefined, open a save file dialog
			 */
			if (IsUnboundNonExistingFile)
			{
				var sf = new Microsoft.Win32.SaveFileDialog();
				sf.FileName = AbsoluteFilePath;

				if (!sf.ShowDialog().Value)
					return false;
				else
					AbsoluteFilePath = sf.FileName;
			}
			try
			{
				Editor.Save(AbsoluteFilePath);
			}
			catch (Exception ex) { ErrorLogger.Log(ex); return false; }
			Modified = false;
			return true;
		}

		public override void Reload()
		{
			if (File.Exists(AbsoluteFilePath))
				Editor.Load(AbsoluteFilePath);

			UpdateSyntaxHighlighter();
			Modified = false;
		}
		#endregion

		void Init()
		{
			AddChild(Editor);
			Editor.Margin = new System.Windows.Thickness(0);
			Editor.BorderBrush = null;

			Reload();

			Editor.ShowLineNumbers = true;
			Editor.TextChanged += new EventHandler(Editor_TextChanged);
			Editor.Document.LineCountChanged += new EventHandler(Document_LineCountChanged);

			// Register Marker strategy
			var tv = Editor.TextArea.TextView;
			MarkerStrategy=new TextMarkerService(Editor);
			tv.Services.AddService(typeof(TextMarkerService), MarkerStrategy);
			tv.LineTransformers.Add(MarkerStrategy);
			tv.BackgroundRenderers.Add(MarkerStrategy);
			

			RefreshErrorHighlightings();
			RefreshBreakpointHighlightings();
		}

		int _LastLineCount;
		void Document_LineCountChanged(object sender, EventArgs e)
		{
			// Relocate breakpoints that are located after the current cursor position
			// Note: Breakpoints can only be relocated when not in debugging mode!
			var affectedBreakpoints = CoreManager.BreakpointManagement.GetBreakpointsAt(RelativeFilePath).Where(bpw => bpw.Line >= Editor.TextArea.Caret.Line);


		}

		#region Syntax Highlighting
		/// <summary>
		/// Associates a new syntax highligther with the editor
		/// </summary>
		public void UpdateSyntaxHighlighter()
		{
			try{
				var hi = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(FileName));
				Editor.SyntaxHighlighting = hi;
			}catch{}
		}

		public static IHighlightingDefinition GetHighlighting(string file)
		{
			return null;
		}

		public class ErrorMarker:TextMarker
		{
			public readonly EditorDocument EditorDocument;
			public readonly GenericError Error;

			public ErrorMarker(EditorDocument EditorDoc, GenericError Error)
				:base(EditorDoc.MarkerStrategy,EditorDoc.Editor.Document.GetOffset(Error.Location.Line,Error.Location.Column),false)
			{
				this.EditorDocument = EditorDoc;
				this.Error = Error;
			}
		}

		public class BreakpointMarker : TextMarker
		{
			public readonly BreakpointWrapper Breakpoint;

			public BreakpointMarker(EditorDocument EditorDoc, BreakpointWrapper breakPoint)
				:base(EditorDoc.MarkerStrategy,EditorDoc.Editor.Document.GetOffset(breakPoint.Line,0),true)
			{
				this.Breakpoint = breakPoint;
			}

			public new int StartOffset{
				get{return base.StartOffset;}
				set{
					//TODO: Is StartOffset really working?
					var newLine=TextMarkerService.Editor.Document.GetLineByOffset(value).LineNumber;
					Breakpoint.Line=newLine;
				}
			}
		}

		public void RefreshErrorHighlightings()
		{
			// Clear old markers
			foreach (var marker in MarkerStrategy.TextMarkers.ToArray())
				if (marker is ErrorMarker)
					marker.Delete();

			foreach (var err in CoreManager.ErrorManagement.GetErrorsForFile(AbsoluteFilePath))
			{
				var m = new ErrorMarker(this, err);
				MarkerStrategy.Add(m);

				m.Redraw();
			}
		}

		public void RefreshBreakpointHighlightings()
		{
			// Clear old markers
			foreach (var marker in MarkerStrategy.TextMarkers.ToArray())
				if (marker is BreakpointMarker)
					marker.Delete();

			var bps = CoreManager.BreakpointManagement.GetBreakpointsAt(AbsoluteFilePath);
			if(bps!=null)
				foreach (var bpw in bps)
				{
					var m = new BreakpointMarker(this, bpw);
					MarkerStrategy.Add(m);

					m.Redraw();
				}
		}
		#endregion

		#region Code Completion

		#endregion

		#region Editor events
		void Editor_TextChanged(object sender, EventArgs e)
		{
			Modified = true;
		}
		#endregion
	}
}
