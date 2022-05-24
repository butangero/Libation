﻿using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.GridView
{
	public class SeriesEntry : GridEntry
	{
		public List<LibraryBookEntry> Children { get; init; }
		public override DateTime DateAdded => Children.Max(c => c.DateAdded);
		public override float SeriesIndex { get; }
		public override string ProductRating
		{
			get
			{
				var productAverageRating = new Rating(Children.Average(c => c.LibraryBook.Book.Rating.OverallRating), Children.Average(c => c.LibraryBook.Book.Rating.PerformanceRating), Children.Average(c => c.LibraryBook.Book.Rating.StoryRating));
				return productAverageRating.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			}
			protected set => throw new NotImplementedException();
		}
		public override string PurchaseDate { get; protected set; }
		public override string MyRating
		{
			get
			{
				var myAverageRating = new Rating(Children.Average(c => c.LibraryBook.Book.UserDefinedItem.Rating.OverallRating), Children.Average(c => c.LibraryBook.Book.UserDefinedItem.Rating.PerformanceRating), Children.Average(c => c.LibraryBook.Book.UserDefinedItem.Rating.StoryRating));
				return myAverageRating.ToStarString()?.DefaultIfNullOrWhiteSpace("");
			}
			protected set => throw new NotImplementedException();
		}
		public override string Series { get; protected set; }
		public override string Title { get; protected set; }
		public override string Length
		{
			get
			{
				int bookLenMins = Children.Sum(c => c.LibraryBook.Book.LengthInMinutes);
				return bookLenMins == 0 ? "" : $"{bookLenMins / 60} hr {bookLenMins % 60} min";
			}
			protected set => throw new NotImplementedException();
		}
		public override string Authors { get; protected set; }
		public override string Narrators { get; protected set; }
		public override string Category { get; protected set; }
		public override string Misc { get; protected set; } = string.Empty;
		public override string Description { get; protected set; } = string.Empty;
		public override string DisplayTags { get; } = string.Empty;

		public override LiberateButtonStatus Liberate { get; }

		protected override Book Book => SeriesBook.Book;

		private SeriesBook SeriesBook { get; set; }

		private SeriesEntry(SeriesBook seriesBook)
		{
			Liberate = new LiberateButtonStatus { IsSeries = true };
			SeriesIndex = seriesBook.Index;
		}
		public SeriesEntry(SeriesBook seriesBook, IEnumerable<LibraryBook> children) : this(seriesBook)
		{
			Children = children.Select(c => new LibraryBookEntry(c) { Parent = this }).OrderBy(c => c.SeriesIndex).ToList();
			SetSeriesBook(seriesBook);
		}
		public SeriesEntry(SeriesBook seriesBook, LibraryBook child) : this(seriesBook)
		{
			Children = new() { new LibraryBookEntry(child) { Parent = this } };
			SetSeriesBook(seriesBook);
		}

		private void SetSeriesBook(SeriesBook seriesBook)
		{
			SeriesBook = seriesBook;
			LoadCover();

			// Immutable properties
			{
				Title = SeriesBook.Series.Name;
				Series = SeriesBook.Series.Name;
				PurchaseDate = Children.Min(c => c.LibraryBook.DateAdded).ToString("d");
				Authors = Book.AuthorNames();
				Narrators = Book.NarratorNames();
				Category = string.Join(" > ", Book.CategoriesNames());
			}
		}


		/// <summary>Create getters for all member object values by name</summary>
		protected override Dictionary<string, Func<object>> CreateMemberValueDictionary() => new()
		{
			{ nameof(Title), () => Book.SeriesSortable() },
			{ nameof(Series), () => Book.SeriesSortable() },
			{ nameof(Length), () => Children.Sum(c => c.LibraryBook.Book.LengthInMinutes) },
			{ nameof(MyRating), () => Children.Average(c => c.LibraryBook.Book.UserDefinedItem.Rating.FirstScore()) },
			{ nameof(PurchaseDate), () => Children.Min(c => c.LibraryBook.DateAdded) },
			{ nameof(ProductRating), () => Children.Average(c => c.LibraryBook.Book.Rating.FirstScore()) },
			{ nameof(Authors), () => string.Empty },
			{ nameof(Narrators), () => string.Empty },
			{ nameof(Description), () => string.Empty },
			{ nameof(Category), () => string.Empty },
			{ nameof(Misc), () => string.Empty },
			{ nameof(DisplayTags), () => string.Empty },
			{ nameof(Liberate), () => Liberate },
			{ nameof(DateAdded), () => DateAdded },
		};
	}
}