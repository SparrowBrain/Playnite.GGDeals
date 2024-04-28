using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
    public interface IAddAGameService
    {
        Task<bool> TryAddToCollection(Game game);
    }
}