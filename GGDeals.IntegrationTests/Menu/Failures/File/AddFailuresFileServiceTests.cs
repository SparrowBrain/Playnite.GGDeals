using AutoFixture.Xunit2;
using GGDeals.Menu.Failures.File;
using GGDeals.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GGDeals.Models;
using Xunit;

namespace GGDeals.IntegrationTests.Menu.Failures.File
{
	public class AddFailuresFileServiceTests
	{
		private const string FailuresFilePath = "failures.json";

		[Fact]
		public async Task Load_ReturnsEmptyDictionary_WhenFileDoesNotExist()
		{
			// Arrange
			EnsureFileDoesNotExist();
			var sut = CreateSut();

			// Act
			var result = await sut.Load();

			// Assert
			Assert.Empty(result);
		}

		[Theory]
		[AutoData]
		public async Task Load_ReturnsContentsOfFile_WhenFileExists(
			Dictionary<Guid, AddResult> contents)
		{
			// Arrange
			EnsureFileExists(contents);
			var sut = CreateSut();

			// Act
			var result = await sut.Load();

			// Assert
			Assert.Equivalent(contents, result);
		}

		[Theory]
		[AutoData]
		public async Task Load_ReturnsContentsOfFile_WhenFileIsV0(
			Dictionary<Guid, AddToCollectionResult> contents)
		{
			// Arrange
			EnsureV0FileExists(contents);
			var sut = CreateSut();

			// Act
			var result = await sut.Load();

			// Assert
			Assert.Equivalent(contents.ToDictionary(x => x.Key, x => new AddResult() { Result = x.Value }), result);
		}

		[Theory]
		[AutoData]
		public async Task Save_CreatesNewFile_WhenFileDoesNotExist(
			Dictionary<Guid, AddResult> newContents)
		{
			// Arrange
			EnsureFileDoesNotExist();
			var sut = CreateSut();

			// Act
			await sut.Save(newContents);

			// Assert
			var result = ReadFile();
			Assert.Equivalent(newContents, result);
		}

		[Theory]
		[AutoData]
		public async Task Save_OverwritesFile_WhenFileExists(
			Dictionary<Guid, AddResult> originalContents,
			Dictionary<Guid, AddResult> newContents)
		{
			// Arrange
			EnsureFileExists(originalContents);
			var sut = CreateSut();

			// Act
			await sut.Save(newContents);

			// Assert
			var result = ReadFile();
			Assert.Equivalent(newContents, result);
		}

		private AddFailuresFileService CreateSut()
		{
			return new AddFailuresFileService(FailuresFilePath);
		}

		private void EnsureFileDoesNotExist()
		{
			if (System.IO.File.Exists(FailuresFilePath))
			{
				System.IO.File.Delete(FailuresFilePath);
			}
		}

		private static void EnsureV0FileExists(Dictionary<Guid, AddToCollectionResult> contents)
		{
			System.IO.File.WriteAllText(FailuresFilePath, JsonConvert.SerializeObject(contents));
		}

		private static void EnsureFileExists(Dictionary<Guid, AddResult> contents)
		{
			var file = new FailuresFile() { Failures = contents };
			System.IO.File.WriteAllText(FailuresFilePath, JsonConvert.SerializeObject(file));
		}

		private static Dictionary<Guid, AddResult> ReadFile()
		{
			var file = JsonConvert.DeserializeObject<FailuresFile>(System.IO.File.ReadAllText(FailuresFilePath));
			return file.Failures;
		}
	}
}