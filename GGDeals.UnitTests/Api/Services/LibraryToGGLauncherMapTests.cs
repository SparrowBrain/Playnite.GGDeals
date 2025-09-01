using GGDeals.Api.Models;
using GGDeals.Api.Services;
using Playnite.SDK.Models;
using System;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Api.Services
{
	public class LibraryToGGLauncherMapTests
	{
		[Theory]
		[MemberAutoMoqData(nameof(LibraryIdTestData))]
		public void GetLibraryName_ReturnsGGLibraryNameFromLibraryId_WhenPluginIdMatchesTheMap(
			string playniteLibraryId,
			GGLauncher ggLauncher,
			LibraryToGGLauncherMap sut)
		{
			// Arrange
			var pluginId = Guid.Parse(playniteLibraryId);

			// Act
			var actual = sut.GetGGLauncher(pluginId);

			// Assert
			Assert.Equal(ggLauncher, actual);
		}

		public static TheoryData<string, GGLauncher> LibraryIdTestData => new TheoryData<string, GGLauncher>
		{
			{ "CB91DFC9-B977-43BF-8E70-55F46E410FAB", GGLauncher.Steam },
			{ "85DD7072-2F20-4E76-A007-41035E390724", GGLauncher.EA },
			{ "C2F038E5-8B92-4877-91F1-DA9094155FC5", GGLauncher.Ubisoft },
			{ "AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E", GGLauncher.GOG },
			{ "00000002-DBD1-46C6-B5D0-B1BA559D10E4", GGLauncher.Epic },
			{ "7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287", GGLauncher.Microsoft },
			{ "E3C26A3D-D695-4CB7-A769-5FF7612C7EDD", GGLauncher.BattleNet },
			{ "88409022-088a-4de8-805a-fdbac291f00a", GGLauncher.Rockstar },
			{ "402674cd-4af6-4886-b6ec-0e695bfa0688", GGLauncher.PrimeGaming },
			{ "e4ac81cb-1b1a-4ec9-8639-9a9633989a71", GGLauncher.Playstation },
			{ "e4ac81cb-1b1a-4ec9-8639-9a9633989a72", GGLauncher.Nintendo },
			{ "00000001-EBB2-4EEC-ABCB-7C89937A42BB", GGLauncher.Itch },
			{ "00000000-0000-0000-0000-000000000000", GGLauncher.Other },
			{ "415373e6-afb4-4be0-8f31-82d9a0b54086", GGLauncher.Other }
		};
	}
}