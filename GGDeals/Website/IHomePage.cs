using System.Threading.Tasks;

namespace GGDeals.Website
{
    public interface IHomePage
    {
        Task<bool> IsUserLoggedIn();

        Task<string> GetUserName();
    }
}