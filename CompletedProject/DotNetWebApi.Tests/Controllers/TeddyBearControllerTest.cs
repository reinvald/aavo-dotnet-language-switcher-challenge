using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using DotNetWebApi.Models;
using DotNetWebApi.Controllers;

namespace DotNetWebApi.Tests;

public class TeddyBearControllerTest
{

    [Fact]
    public async void CreateTeddyBearShouldCallDBContextCorrectly()
    {
        var teddyBearContext = new Mock<TeddyBearsContext>(new DbContextOptionsBuilder<TeddyBearsContext>().Options);

        var mockTeddyBearDbSet = new Mock<DbSet<TeddyBear>>();
        teddyBearContext.Setup(m => m.TeddyBears).Returns(mockTeddyBearDbSet.Object);

        var controller = new TeddyBearController(teddyBearContext.Object);

        var newTeddyBear = new TeddyBear();
        await controller.CreateTeddyBear(newTeddyBear);

        teddyBearContext.Verify(m => m.TeddyBears, Times.Once());
        teddyBearContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        teddyBearContext.Verify(m => m.TeddyBears.Add(newTeddyBear), Times.Once());
    }
}