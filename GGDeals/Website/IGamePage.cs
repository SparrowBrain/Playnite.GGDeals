using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Website
{
    public interface IGamePage
    {
        Task ClickOwnItButton();

        Task ExpandDrmDropDown();

        Task ClickDrmPlatformCheckBox(Game game);

        Task ClickSubmitOwnItForm();
    }
}