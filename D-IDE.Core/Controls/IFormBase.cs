﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace D_IDE.Core.Controls
{
	public interface IFormBase
	{
		AvalonDock.DockingManager DockManager { get; }
		System.Windows.Threading.Dispatcher Dispatcher { get; }


		void RefreshMenu();
		void RefreshGUI();
		void RefreshErrorList();
		void RefreshTitle();
		void RefreshProjectExplorer();

		string LeftStatusText { get; set; }
	}

	public static class IDEUICommands
	{
		public static readonly RoutedUICommand GoTo = new RoutedUICommand("Go to line", "GoTo", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.G, ModifierKeys.Control) }));

		public static readonly RoutedUICommand SaveAll = new RoutedUICommand("Save all documents", "SaveAll", typeof(Window));
		public static readonly RoutedUICommand CommentBlock = new RoutedUICommand("Comment code", "CommentBlock", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.K, ModifierKeys.Control) }));

		public static readonly RoutedUICommand UncommentBlock = new RoutedUICommand("Uncomment code", "UncommentBlock", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.K, ModifierKeys.Control|ModifierKeys.Shift) }));

		public static readonly RoutedUICommand DuplicateLine = new RoutedUICommand("Duplicate current line", "DuplicateLine", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.D, ModifierKeys.Control) }));

		public static readonly RoutedUICommand ReformatDoc = new RoutedUICommand("Reformat current document", "ReformatDoc", typeof(Window));

		public static readonly RoutedUICommand ToggleBreakpoint = new RoutedUICommand("Toggle breakpoint", "ToggleBreakpoint", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.F9) }));

		public static readonly RoutedUICommand StepIn = new RoutedUICommand("", "StepIn", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.F11 )}));

		public static readonly RoutedUICommand StepOver = new RoutedUICommand("", "StepOver", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.F10) }));

		public static readonly RoutedUICommand LaunchDebugger = new RoutedUICommand("", "LaunchDebugger", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.F5) }));

		public static readonly RoutedUICommand LaunchWithoutDebugger = new RoutedUICommand("", "LaunchWithoutDebugger", typeof(Window),
			new InputGestureCollection(new[] { new KeyGesture(Key.F5, ModifierKeys.Shift) }));
	}
}
