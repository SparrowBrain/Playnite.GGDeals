using System.Threading.Tasks;
using GGDeals.Services;
using Playnite.SDK.Models;

namespace GGDeals.Website
{
    public interface IGamePage
    {
        Task ClickOwnItButton();

        Task ExpandDrmDropDown();

        Task<bool> IsDrmPlatformCheckboxActive(Game game);

        Task ClickDrmPlatformCheckBox(Game game);

        Task ClickSubmitOwnItForm();
    }
}