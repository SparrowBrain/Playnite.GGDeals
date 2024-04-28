using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Website
{
    public interface IGGWebsite
    {
        Task CheckLoggedIn();

        Task<bool> TryNavigateToGamePage(Game game);
    }
}