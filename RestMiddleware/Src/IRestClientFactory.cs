namespace RestMiddleware.Src
{
    public interface IRestClientFactory
    {
        RestClient CreateClient();
        RestClient CreateClient(string name);
    }
}
