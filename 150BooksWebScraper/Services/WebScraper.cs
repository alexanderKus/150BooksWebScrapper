using System.Net;
using _150BooksWebScraper.Interfaces;
using _150BooksWebScraper.Models;
using HtmlAgilityPack;

namespace _150BooksWebScraper.Services;

public class WebScraper(string url) : IScraper
{
    private readonly string _url = url;
    private readonly HttpClient _http = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
    });

    public async Task<List<Book>> Perform()
    {
        var document = await Get150BooksDocument();
        var nodes = GetNodes(document);
        List<Book> books = [];
        if (nodes is not null)
            books = await GetBooks(nodes);
        return books;
    }

    private async Task<HtmlDocument> Get150BooksDocument()
    {
        var html = await _http.GetStringAsync(_url);
        HtmlDocument document = new();
        document.LoadHtml(html);
        return document;
    }
    
    private HtmlNode[]? GetNodes(HtmlDocument document)
    {
        var container = document.DocumentNode.SelectSingleNode("""//*[@id="recommended-books"]/div""");
        var booksHtml = container.SelectNodes("./div");
        var nodes = booksHtml?.Select(book 
                => book.SelectSingleNode("./div")
                    .SelectSingleNode("./figure")
                    .SelectSingleNode("./a"))
            .ToArray();
        return nodes;
    }

    private async Task<List<Book>> GetBooks(HtmlNode[] nodes)
    {
        ConfigurateHttpClientToAccessAmazon();

        List<Book> books = [];
        foreach (var node in nodes)
        {
            var book = await GetBook(node);
            if (book is not null)
                books.Add((Book)book);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        return books;
    }
    
    private async Task<Book?> GetBook(HtmlNode node)
    {
        var href = node.GetAttributeValue("href", "No href found");
        try
        {
            var amazonPageHtml = await _http.GetStringAsync(href);
            HtmlDocument amazonDocument = new();
            amazonDocument.LoadHtml(amazonPageHtml);
            var title = amazonDocument.DocumentNode.SelectSingleNode("""//*[@id="productTitle"]""").InnerHtml;
            var author = amazonDocument.DocumentNode.SelectSingleNode("""//*[@id="bylineInfo"]/span[1]/a""").InnerHtml;
            Console.WriteLine($"[INFO] Title: {title}, Author: {author}");
            return new Book { Title = title, Author = author };
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] Cannot get value for: {href}");
            return null;
        }
    }

    private void ConfigurateHttpClientToAccessAmazon()
    {
        _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        _http.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        _http.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _http.DefaultRequestHeaders.Add("Connection", "keep-alive");
        _http.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
    }
}