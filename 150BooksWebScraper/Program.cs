using System.Globalization;
using _150BooksWebScraper.Interfaces;
using _150BooksWebScraper.Services;
using CsvHelper;

IScraper scraper = new WebScraper(@"https://powerseductionandwar.com/books/");
var books = await scraper.Perform();

using var writer = new StreamWriter("books.csv");
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
csv.WriteRecords(books);

Console.WriteLine("Done");