class ArticleSource
{
    public ArticleSource(Articulo article, Fuente source)
    {
        Article = article;
        Source = source;
    }
    public Articulo Article { get; set; }
    public Fuente Source { get; set; }
}