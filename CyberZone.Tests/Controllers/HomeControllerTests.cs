using CyberZone.Application.Common;
using CyberZone.Application.DTOs;
using CyberZone.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;

namespace CyberZone.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IClubService> _mockClubService;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _mockClubService = new Mock<IClubService>();
        _controller = new HomeController(_mockClubService.Object);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        var result = _controller.Privacy();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Catalog_ReturnsViewWithClubCatalogDtos()
    {
        var clubs = new List<ClubCatalogDto>
        {
            new() { Id = Guid.NewGuid(), Name = "CyberArena", MinPrice = 50m, Rating = 4.5 },
            new() { Id = Guid.NewGuid(), Name = "GameZone", MinPrice = 30m, Rating = 4.0 }
        };
        _mockClubService
            .Setup(s => s.GetClubsForCatalogAsync())
            .ReturnsAsync(Result.Success<IEnumerable<ClubCatalogDto>>(clubs));

        var result = await _controller.Catalog();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeEquivalentTo(clubs);
    }

    [Fact]
    public async Task Catalog_CallsClubServiceOnce()
    {
        _mockClubService
            .Setup(s => s.GetClubsForCatalogAsync())
            .ReturnsAsync(Result.Success<IEnumerable<ClubCatalogDto>>(new List<ClubCatalogDto>()));

        await _controller.Catalog();

        _mockClubService.Verify(s => s.GetClubsForCatalogAsync(), Times.Once);
    }

    [Fact]
    public async Task Catalog_EmptyResult_ReturnsViewWithEmptyList()
    {
        _mockClubService
            .Setup(s => s.GetClubsForCatalogAsync())
            .ReturnsAsync(Result.Success<IEnumerable<ClubCatalogDto>>(Enumerable.Empty<ClubCatalogDto>()));

        var result = await _controller.Catalog();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeAssignableTo<IEnumerable<ClubCatalogDto>>()
            .Which.Should().BeEmpty();
    }
}
