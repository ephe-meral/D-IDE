﻿using System;
using System.IO;
using System.Linq;
using D_IDE.Core;
using D_IDE.Core.Controls.Editor;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using D_Parser.Core;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Windows.Controls;
using D_Parser;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Input;
using D_IDE.Core.Controls;
using System.Windows;

namespace D_IDE.D
{
	public class DEditorDocument:EditorDocument
	{
		#region Properties
		IAbstractSyntaxTree _unboundTree;
		public IAbstractSyntaxTree SyntaxTree { 
			get {
				if (HasProject)
				{
					var prj = Project as DProject;
					if(prj!=null)
						return prj.ParsedModules[AbsoluteFilePath];
				}

				return _unboundTree;
			}
			set {
				if(value!=null)
				value.FileName = AbsoluteFilePath;
				if (HasProject)
				{
					var prj = Project as DProject;
					if (prj != null)
						prj.ParsedModules[AbsoluteFilePath]=value;
				}
				_unboundTree=value;
			}
		}

		ToolTip editorToolTip = new ToolTip();
		#endregion

		public DEditorDocument()
		{
			Init();
		}

		public DEditorDocument(string file)
			: base(file)
		{
			Init();
		}

		void Init()
		{
			// Register CodeCompletion events
			Editor.TextArea.TextEntering += new System.Windows.Input.TextCompositionEventHandler(TextArea_TextEntering);
			Editor.TextArea.TextEntered += new System.Windows.Input.TextCompositionEventHandler(TextArea_TextEntered);
			Editor.Document.TextChanged += new EventHandler(Document_TextChanged);
			Editor.TextArea.Caret.PositionChanged += new EventHandler(TextArea_SelectionChanged);
			Editor.MouseHover += new System.Windows.Input.MouseEventHandler(Editor_MouseHover);
			Editor.MouseHoverStopped += new System.Windows.Input.MouseEventHandler(Editor_MouseHoverStopped);

			//TODO: Modify the layout - add two selection combo boxes to the editor view
			// One for selecting types that were declared in the module
			// The second for the type's members

			#region Init context menu
			var cm = new ContextMenu();
			Editor.ContextMenu = cm;

			var cmi = new MenuItem() { Header = "Add import directive", ToolTip="Add an import directive to the document if type cannot be resolved currently"};
			cmi.Click += ContextMenu_AddImportStatement_Click;
			cm.Items.Add(cmi);

			cmi = new MenuItem() { Header = "Go to definition", ToolTip = "Go to the definition that defined the currently hovered item" };
			cmi.Click += new System.Windows.RoutedEventHandler(ContextMenu_GotoDefinition_Click);
			cm.Items.Add(cmi);

			cmi = new MenuItem() { Header = "Toggle Breakpoint", 
				ToolTip = "Toggle breakpoint on the currently selected line",
				Command=D_IDE.Core.Controls.IDEUICommands.ToggleBreakpoint
			};
			cm.Items.Add(cmi);

			cm.Items.Add(new Separator());

			cmi = new MenuItem()
			{
				Header = "Comment selection",
				ToolTip = "Comment out current selection. If nothing is selected, the current line will be commented only",
				Command = D_IDE.Core.Controls.IDEUICommands.CommentBlock
			};
			cm.Items.Add(cmi);

			cmi = new MenuItem()
			{
				Header = "Uncomment selection",
				ToolTip = "Uncomment current block. The nearest comment tags will be removed.",
				Command = D_IDE.Core.Controls.IDEUICommands.UncommentBlock
			};
			cm.Items.Add(cmi);

			cm.Items.Add(new Separator());

			cmi = new MenuItem(){	Header = "Cut",	Command = System.Windows.Input.ApplicationCommands.Cut	};
			cm.Items.Add(cmi);

			cmi = new MenuItem() { Header = "Copy", Command = System.Windows.Input.ApplicationCommands.Copy };
			cm.Items.Add(cmi);

			cmi = new MenuItem() { Header = "Paste", Command = System.Windows.Input.ApplicationCommands.Paste };
			cm.Items.Add(cmi);
			#endregion

			CommandBindings.Add(new CommandBinding(IDEUICommands.CommentBlock,CommentBlock));
			CommandBindings.Add(new CommandBinding(IDEUICommands.UncommentBlock,UncommentBlock));

			Parse();
		}

		void CommentBlock(object s, ExecutedRoutedEventArgs e)
		{
			if (Editor.SelectionLength<1)
			{
				Editor.Document.Insert(Editor.Document.GetOffset(Editor.TextArea.Caret.Line,0),"//");
			}
			else
			{
				Editor.Document.UndoStack.StartUndoGroup();

				bool a, b, IsInBlock, IsInNested;
				DCodeResolver.Commenting.IsInCommentAreaOrString(Editor.Text,Editor.SelectionStart, out a, out b, out IsInBlock, out IsInNested);

				if (!IsInBlock && !IsInNested)
				{
					Editor.Document.Insert(Editor.SelectionStart+Editor.SelectionLength, "*/");
					Editor.Document.Insert(Editor.SelectionStart, "/*");
				}
				else
				{
					Editor.Document.Insert(Editor.SelectionStart + Editor.SelectionLength, "+/");
					Editor.Document.Insert(Editor.SelectionStart, "/+");
				}

				Editor.SelectionLength -= 2;

				Editor.Document.UndoStack.EndUndoGroup();
			}
		}

		void UncommentBlock(object s, ExecutedRoutedEventArgs e)
		{
			var CaretOffset = Editor.CaretOffset;
			#region Remove line comments first
			var ls = Editor.Document.GetLineByNumber(Editor.TextArea.Caret.Line);
			int commStart = CaretOffset;
			for (; commStart > ls.Offset; commStart--)
			{
				if (Editor.Document.GetCharAt(commStart) == '/' && commStart > 0 &&
					Editor.Document.GetCharAt(commStart - 1) == '/')
				{
					Editor.Document.Remove(commStart - 1, 2);
					return;
				}
			}
			#endregion
			#region If no single-line comment was removed, delete multi-line comment block tags
			if (CaretOffset < 2) return;
			int off = CaretOffset - 2;

			// Seek the comment block opener
			commStart = DCodeResolver.Commenting.LastIndexOf(Editor.Text, false, off);
			int nestedCommStart = DCodeResolver.Commenting.LastIndexOf(Editor.Text, true, off);
			if (commStart < 0 && nestedCommStart < 0) return;

			// Seek the fitting comment block closer
			int off2 = off + (Math.Max(nestedCommStart, commStart) == off ? 2 : 0);
			int commEnd = DCodeResolver.Commenting.IndexOf(Editor.Text, false, off2);
			int commEnd2 = DCodeResolver.Commenting.IndexOf(Editor.Text, true, off2);

			if (nestedCommStart > commStart && commEnd2 > nestedCommStart)
			{
				commStart = nestedCommStart;
				commEnd = commEnd2;
			}

			if (commStart < 0 || commEnd < 0) return;

			Editor.Document.UndoStack.StartUndoGroup();
			Editor.Document.Remove(commEnd, 2);
			Editor.Document.Remove(commStart, 2);

			if (commStart != off) Editor.CaretOffset = off;

			Editor.Document.UndoStack.EndUndoGroup();
			#endregion
		}

		void ContextMenu_GotoDefinition_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			try
			{
				var types = DCodeResolver.ResolveTypeDeclarations(
					SyntaxTree,
					Editor.Text,
					Editor.CaretOffset,
					new CodeLocation(Editor.TextArea.Caret.Column, Editor.TextArea.Caret.Line),
					false,
					DCodeCompletionSupport.EnumAvailableModules(this) // std.cstream.din.getc(); <<-- It's resolvable but not imported explictily! So also scan the global cache!
					//DCodeResolver.ResolveImports(EditorDocument.SyntaxTree,EnumAvailableModules(EditorDocument))
					,true
					).ToArray();

				INode n = null;
				// If there are multiple types, show a list of those items
				if (types.Length > 1)
				{
					var dlg = new ListSelectionDialog();
					
					var l = new List<string>();
					int j = 0;
					foreach (var i in types)
						l.Add("("+(++j).ToString()+") "+i.ToString()); // Bug: To make items unique (which is needed for the listbox to run properly), it's needed to add some kind of an identifier to the beginning of the string
					dlg.List.ItemsSource = l;

					dlg.List.SelectedIndex = 0;

					if (dlg.ShowDialog().Value)
					{
						n = types[dlg.List.SelectedIndex];
					}
				}
				else
					n = types[0];

				var mod = n.NodeRoot as IAbstractSyntaxTree;
				if (mod == null)
					return;
				var ed = CoreManager.Instance.OpenFile(mod.FileName) as EditorDocument;
				if (ed != null)
				{
					ed.Editor.CaretOffset=ed.Editor.Document.GetOffset(n.StartLocation.Line,n.StartLocation.Column);
					ed.Editor.ScrollTo(n.StartLocation.Line,n.StartLocation.Column);
				}
			}
			catch { }
		}

		void ContextMenu_AddImportStatement_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
		}

		bool KeysTyped = false;
		Thread parseThread = null;
		void Document_TextChanged(object sender, EventArgs e)
		{
			if (parseThread == null || !parseThread.IsAlive)
			{
				// This thread will continously check if the file was modified.
				// If so, it'll reparse
				parseThread = new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					while (true)
					{
						// While no keys were typed, do nothing
						while (!KeysTyped)
							Thread.Sleep(50);

						// Reset keystyped state for waiting again
						KeysTyped = false;

						// If a key was typed, wait.
						Thread.Sleep(1500);

						// If no other key was typed after waiting, parse the file
						if (KeysTyped)
							continue;

						// Prevent parsing it again; Assign 'false' to it before parsing the document, so if something was typed while parsing, it'll simply parse again
						KeysTyped = false;

						Parse();
					}
				});
				parseThread.Start();
			}

			KeysTyped = true;
		}

		#region Code Completion

		public string ProposedModuleName
		{
			get {
				if (HasProject)
					return Path.ChangeExtension(RelativeFilePath, null).Replace('\\', '.');
				else 
					return Path.GetFileNameWithoutExtension(FileName);
			}
		}

		/// <summary>
		/// Parses the current document content
		/// </summary>
		public void Parse()
		{
			Dispatcher.BeginInvoke(new Util.EmptyDelegate(()=>{
				try{
					if (SyntaxTree != null)
						lock (SyntaxTree)
							DParser.UpdateModuleFromText(SyntaxTree, Editor.Text);
					else
						SyntaxTree =DParser.ParseString(Editor.Text);
					SyntaxTree.FileName = AbsoluteFilePath;
					SyntaxTree.ModuleName = ProposedModuleName;

					UpdateBlockCompletionData();
				}catch(Exception ex){ErrorLogger.Log(ex,ErrorType.Warning,ErrorOrigin.System);}
				CoreManager.ErrorManagement.RefreshErrorList();
			}));
		}

		public override System.Collections.Generic.IEnumerable<GenericError> ParserErrors
		{
			get
			{
				if (SyntaxTree != null)
					foreach (var pe in SyntaxTree.ParseErrors)
						yield return new DParseError(pe) { Project=HasProject?Project:null, FileName=AbsoluteFilePath};
			}
		}

		public CodeLocation CaretLocation
		{
			get { return new CodeLocation(Editor.TextArea.Caret.Column,Editor.TextArea.Caret.Line); }
		}

		IBlockNode lastSelectedBlock = null;
		IEnumerable<ICompletionData> currentEnvCompletionData = null;

		public void UpdateBlockCompletionData()
		{
			var curBlock = DCodeResolver.SearchBlockAt(SyntaxTree, CaretLocation);
			if (curBlock != lastSelectedBlock)
			{
				currentEnvCompletionData = null;

				// If different code blocks was selected, 
				// update the list of items that are available in the current scope
				var l = new List<ICompletionData>();
				DCodeCompletionSupport.Instance.BuildCompletionData(this, l, null);
				currentEnvCompletionData = l;
			}
			curBlock = lastSelectedBlock;
		}

		void TextArea_SelectionChanged(object sender, EventArgs e)
		{
			UpdateBlockCompletionData();
		}

		CompletionWindow completionWindow;
		InsightWindow insightWindow;

		void ShowCodeCompletionWindow(string EnteredText)
		{
			try
			{
				if (!DCodeCompletionSupport.Instance.CanShowCompletionWindow(this) || Editor.IsReadOnly)
					return;

				/*
				 * Note: Once we opened the completion list, it's not needed to care about a later refill of that list.
				 * The completionWindow will search the items that are partly typed into the editor automatically and on its own.
				 * - So there's just an initial filling required.
				 */

				var ccs = DCodeCompletionSupport.Instance;

				if (completionWindow != null)
					return;

				Dispatcher.Invoke(new Util.EmptyDelegate(()=>
				{
					Thread.CurrentThread.IsBackground = true;

					completionWindow = new CompletionWindow(Editor.TextArea);
					completionWindow.CloseAutomatically = true;

					if (string.IsNullOrEmpty(EnteredText))
						foreach (var i in currentEnvCompletionData)
							completionWindow.CompletionList.CompletionData.Add(i);
					else
						ccs.BuildCompletionData(this, completionWindow.CompletionList.CompletionData, EnteredText);

					// If no data present, return
					if (completionWindow.CompletionList.CompletionData.Count < 1)
					{
						completionWindow = null;
						return;
					}

					completionWindow.Closed += (object o, EventArgs _e) => { completionWindow = null; }; // After the window closed, reset it to null
					completionWindow.Show();

				}));
			}
			catch (Exception ex) { ErrorLogger.Log(ex); }
		}

		void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			// Return also if there are parser errors - just to prevent crashes
			if (string.IsNullOrWhiteSpace(e.Text) || (SyntaxTree!=null && SyntaxTree.ParseErrors!=null && SyntaxTree.ParseErrors.Count()>0)) return;

			if (completionWindow != null)
			{
				// If entered key isn't part of the identifier anymore, close the completion window and insert the item text.
				if (!DCodeCompletionSupport.Instance. IsIdentifierChar(e.Text[0]))
					completionWindow.CompletionList.RequestInsertion(e);
			}

			// Note: Show completion window even before the first key has been processed by the editor!
			else if(e.Text!=".")
				ShowCodeCompletionWindow(e.Text);
		}

		void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (e.Text == ".") // Show the cc window after the dot has been inserted in the text because the cc win would overwrite it anyway
				ShowCodeCompletionWindow(e.Text);
		}
		#endregion

		#region Editor events
		void Editor_TextChanged(object sender, EventArgs e)
		{
			Modified = true;

			// Relocate/Update build errors
			foreach (var m in MarkerStrategy.TextMarkers)
			{
				var bem = m as ErrorMarker;
				if(bem==null)
					continue;

				var nloc=bem.EditorDocument.Editor.Document.GetLocation(bem.StartOffset);
				bem.Error.Line = nloc.Line;
				bem.Error.Column = nloc.Column;
			}			
		}

		void Document_LineCountChanged(object sender, EventArgs e)
		{
			// Relocate breakpoint positions - when not being in debug mode!
			if (!CoreManager.DebugManagement.IsDebugging)
				foreach (var mk in MarkerStrategy.TextMarkers)
				{
					var bpm = mk as BreakpointMarker;
					if (bpm != null)
					{
						bpm.Breakpoint.Line = Editor.Document.GetLineByOffset(bpm.StartOffset).LineNumber;
					}
				}
		}

		#region Document ToolTips
		void Editor_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e)
		{
			editorToolTip.IsOpen = false;
		}

		void Editor_MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var edpos = e.GetPosition(Editor);
			var pos = Editor.GetPositionFromPoint(edpos);
			if (pos.HasValue)
			{
				// Avoid showing a tooltip if the cursor is located after a line-end
				var vpos = Editor.TextArea.TextView.GetVisualPosition(new TextViewPosition(pos.Value.Line, Editor.Document.GetLineByNumber(pos.Value.Line).TotalLength), ICSharpCode.AvalonEdit.Rendering.VisualYPosition.LineMiddle);
				// Add TextView position to Editor-related point
				vpos = Editor.TextArea.TextView.TranslatePoint(vpos, Editor);

				var ttArgs = new ToolTipRequestArgs(edpos.X <= vpos.X, pos.Value);
				try
				{
					DCodeCompletionSupport.Instance.BuildToolTip(this, ttArgs);
				}
				catch (Exception ex)
				{
					ErrorLogger.Log(ex);
					return;
				}

				// If no content present, close and return
				if (ttArgs.ToolTipContent == null)
				{
					editorToolTip.IsOpen = false;
					return;
				}

				editorToolTip.PlacementTarget = this; // required for property inheritance
				editorToolTip.Content = ttArgs.ToolTipContent;
				editorToolTip.IsOpen = true;
				e.Handled = true;
			}
		}
		#endregion
		#endregion
	}

	public class ToolTipRequestArgs
	{
		public ToolTipRequestArgs(bool isDoc, TextViewPosition pos)
		{
			InDocument = isDoc;
			Position = pos;
		}

		public bool InDocument { get; protected set; }
		public TextViewPosition Position { get; protected set; }
		public int Line { get { return Position.Line; } }
		public int Column { get { return Position.Column; } }
		public object ToolTipContent { get; set; }
	}
}
