﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using Dinah.Core.Threading;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
	public partial class Form1
	{
		protected void Configure_VisibleBooks()
		{
			// init formattable
			visibleCountLbl.Format(0);
			liberateVisibleToolStripMenuItem.Format(0);
			liberateVisible2ToolStripMenuItem.Format(0);

			// bottom-left visible count
			productsGrid.VisibleCountChanged += (_, qty) => visibleCountLbl.Format(qty);

			// top menu strip
			visibleBooksToolStripMenuItem.Format(0);
			productsGrid.VisibleCountChanged += (_, qty) => {
				visibleBooksToolStripMenuItem.Format(qty);
				visibleBooksToolStripMenuItem.Enabled = qty > 0;

				var notLiberatedCount = productsGrid.GetVisible().Count(lb => lb.Book.UserDefinedItem.BookStatus == DataLayer.LiberatedStatus.NotLiberated);
			};

			productsGrid.VisibleCountChanged += setLiberatedVisibleMenuItemAsync;
			LibraryCommands.BookUserDefinedItemCommitted += setLiberatedVisibleMenuItemAsync;
		}
		private async void setLiberatedVisibleMenuItemAsync(object _, int __)
			=> await Task.Run(setLiberatedVisibleMenuItem);
		private async void setLiberatedVisibleMenuItemAsync(object _, EventArgs __)
			=> await Task.Run(setLiberatedVisibleMenuItem);
		void setLiberatedVisibleMenuItem()
		{
			var notLiberated = productsGrid.GetVisible().Count(lb => lb.Book.UserDefinedItem.BookStatus == DataLayer.LiberatedStatus.NotLiberated);
			this.UIThreadSync(() =>
			{
				if (notLiberated > 0)
				{
					liberateVisibleToolStripMenuItem.Format(notLiberated);
					liberateVisibleToolStripMenuItem.Enabled = true;

					liberateVisible2ToolStripMenuItem.Format(notLiberated);
					liberateVisible2ToolStripMenuItem.Enabled = true;
				}
				else
				{
					liberateVisibleToolStripMenuItem.Text = "All visible books are liberated";
					liberateVisibleToolStripMenuItem.Enabled = false;

					liberateVisible2ToolStripMenuItem.Text = "All visible books are liberated";
					liberateVisible2ToolStripMenuItem.Enabled = false;
				}
			});
		}

		private async void liberateVisible(object sender, EventArgs e)
			=> await Task.Run(() => processBookQueue1.AddDownloadDecrypt(productsGrid.GetVisible()));

		private void replaceTagsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var dialog = new TagsBatchDialog();
			var result = dialog.ShowDialog();
			if (result != DialogResult.OK)
				return;

			var visibleLibraryBooks = productsGrid.GetVisible();

			var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
				visibleLibraryBooks,
				$"Are you sure you want to replace tags in {0}?",
				"Replace tags?");

			if (confirmationResult != DialogResult.Yes)
				return;

			foreach (var libraryBook in visibleLibraryBooks)
				libraryBook.Book.UserDefinedItem.Tags = dialog.NewTags;
			LibraryCommands.UpdateUserDefinedItem(visibleLibraryBooks.Select(lb => lb.Book));
		}

		private void setDownloadedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var dialog = new LiberatedStatusBatchDialog();
			var result = dialog.ShowDialog();
			if (result != DialogResult.OK)
				return;

			var visibleLibraryBooks = productsGrid.GetVisible();

			var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
				visibleLibraryBooks,
				$"Are you sure you want to replace downloaded status in {0}?",
				"Replace downloaded status?");

			if (confirmationResult != DialogResult.Yes)
				return;

			foreach (var libraryBook in visibleLibraryBooks)
				libraryBook.Book.UserDefinedItem.BookStatus = dialog.BookLiberatedStatus;
			LibraryCommands.UpdateUserDefinedItem(visibleLibraryBooks.Select(lb => lb.Book));
		}

		private async void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var visibleLibraryBooks = productsGrid.GetVisible();

			var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
				visibleLibraryBooks,
				$"Are you sure you want to remove {0} from Libation's library?",
				"Remove books from Libation?");

			if (confirmationResult != DialogResult.Yes)
				return;

			var visibleIds = visibleLibraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			await LibraryCommands.RemoveBooksAsync(visibleIds);
		}
	}
}