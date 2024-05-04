using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Website
{
    public interface IGGWebsite
    {
        Task NavigateToHomePage();

        Task<bool> TryNavigateToGamePage(Game game);
    }
}