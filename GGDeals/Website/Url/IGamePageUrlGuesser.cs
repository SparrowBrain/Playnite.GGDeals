using Playnite.SDK.Models;

namespace GGDeals.Website.Url
{
    public interface IGamePageUrlGuesser
    {
        string Resolve(Game game);
    }
}