var crawler = new Crawler();
List<string> searchUrls = ["https://www.elespectador.com/"];

foreach(string url in searchUrls)
{
    await crawler.Crawl(url);
}
