﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core.Logging;
using LibationWinForms.Dialogs;
using Serilog;

namespace LibationWinForms
{
    public static class MessageBoxLib
	{
		/// <summary>
		/// Logs error. Displays a message box dialog with specified text and caption.
		/// </summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="exception">Exception to log</param>
		/// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
		public static DialogResult ShowAdminAlert(string text, string caption, Exception exception)
		{
			try
			{
				Serilog.Log.Logger.Error(exception, "Alert admin error: {@DebugText}", new { text, caption });
			}
			catch { }

			using var form = new MessageBoxAlertAdminDialog(text, caption, exception);
			return form.ShowDialog();
		}

		public static void VerboseLoggingWarning_ShowIfTrue()
		{
			// when turning on debug (and especially Verbose) to share logs, some privacy settings may not be obscured
			if (Log.Logger.IsVerboseEnabled())
				MessageBox.Show(@"
Warning: verbose logging is enabled.

This should be used for debugging only. It creates many
more logs and debug files, neither of which are as
strictly anonymous.

When you are finished debugging, it's highly recommended
to set your debug MinimumLevel to Information and restart
Libation.
".Trim(), "Verbose logging enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public static DialogResult ShowConfirmationDialog(IEnumerable<LibraryBook> libraryBooks, string format, string title)
		{
			if (libraryBooks is null || !libraryBooks.Any())
				return DialogResult.Cancel;

			var count = libraryBooks.Count();

			string thisThese = count > 1 ? "these" : "this";
			string bookBooks = count > 1 ? "books" : "book";
			string titlesAgg = libraryBooks.AggregateTitles();

			var message
				= string.Format(format, $"{thisThese} {count} {bookBooks}")
				+ $"\r\n\r\n{titlesAgg}";
			return MessageBox.Show(
				message,
				title,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1);
		}
	}
}
