using GGDeals.Models;
using Playnite.SDK.Models;
using System;

namespace GGDeals.Services
{
    public interface IGameStatusService
    {
        AddToCollectionResult GetStatus(Game game);

        void UpdateStatus(Game game, AddToCollectionResult status);

        IDisposable BufferedUpdate();
    }
}