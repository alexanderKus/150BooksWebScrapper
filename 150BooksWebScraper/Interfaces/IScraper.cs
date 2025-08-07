using _150BooksWebScraper.Models;

namespace _150BooksWebScraper.Interfaces;

public interface IScraper
{
    public Task<List<Book>> Perform();
}
