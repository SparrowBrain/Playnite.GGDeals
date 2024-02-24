using System.Threading.Tasks;

namespace GGDeals.Website
{
    public interface IGGWebsite
    {
        Task<bool> TryNavigateToGamePage(string gameName, out IGamePage gamePage);
    }
}