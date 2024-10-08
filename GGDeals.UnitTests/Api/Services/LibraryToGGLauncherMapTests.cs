﻿using AutoFixture.Xunit2;
using GGDeals.Api.Models;
using GGDeals.Api.Services;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Api.Services
{
	public class LibraryToGGLauncherMapTests
	{
		[Theory]
		[InlineAutoMoqData("CB91DFC9-B977-43BF-8E70-55F46E410FAB", GGLauncher.Steam)]
		[InlineAutoMoqData("85DD7072-2F20-4E76-A007-41035E390724", GGLauncher.EA)]
		[InlineAutoMoqData("C2F038E5-8B92-4877-91F1-DA9094155FC5", GGLauncher.Ubisoft)]
		[InlineAutoMoqData("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E", GGLauncher.GOG)]
		[InlineAutoMoqData("00000002-DBD1-46C6-B5D0-B1BA559D10E4", GGLauncher.Epic)]
		[InlineAutoMoqData("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287", GGLauncher.Microsoft)]
		[InlineAutoMoqData("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD", GGLauncher.BattleNet)]
		[InlineAutoMoqData("88409022-088a-4de8-805a-fdbac291f00a", GGLauncher.Rockstar)]
		[InlineAutoMoqData("402674cd-4af6-4886-b6ec-0e695bfa0688", GGLauncher.PrimeGaming)]
		[InlineAutoMoqData("e4ac81cb-1b1a-4ec9-8639-9a9633989a71", GGLauncher.Playstation)]
		[InlineAutoMoqData("e4ac81cb-1b1a-4ec9-8639-9a9633989a72", GGLauncher.Nintendo)]
		[InlineAutoMoqData("00000001-EBB2-4EEC-ABCB-7C89937A42BB", GGLauncher.Itch)]
		[InlineAutoMoqData("00000000-0000-0000-0000-000000000000", GGLauncher.Other)]
		[InlineAutoMoqData("415373e6-afb4-4be0-8f31-82d9a0b54086", GGLauncher.Other)]
		public void GetLibraryName_ReturnsGGLibraryNameFromLibraryId_WhenPluginIdMatchesTheMap(
			string playniteLibraryId,
			GGLauncher ggLauncher,
			[Frozen] Mock<IAddons> addonsMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Game game,
			LibraryToGGLauncherMap sut)
		{
			// Arrange
			game.PluginId = Guid.Parse(playniteLibraryId);
			playniteApiMock.Setup(x => x.Addons).Returns(addonsMock.Object);
			addonsMock.Setup(x => x.Plugins).Returns(new List<Plugin>());

			// Act
			var actual = sut.GetGGLauncher(game);

			// Assert
			Assert.Equal(ggLauncher, actual);
		}
	}
}