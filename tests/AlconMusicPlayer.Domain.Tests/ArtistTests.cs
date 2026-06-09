using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Domain.Tests;

public class ArtistTests
{
    [Fact]
    public void Create_Should_Return_Artist_With_Given_Name()
    {
        var artist = Artist.Create("Pink Floyd");

        Assert.NotNull(artist);
        Assert.Equal("Pink Floyd", artist.Name);
    }

    [Fact]
    public void Create_Should_Assign_New_Non_Empty_Guid()
    {
        var artist = Artist.Create("Coldplay");

        Assert.NotEqual(Guid.Empty, artist.Id);
    }

    [Fact]
    public void Create_Should_Generate_Unique_Ids()
    {
        var a1 = Artist.Create("A1");
        var a2 = Artist.Create("A2");

        Assert.NotEqual(a1.Id, a2.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_Name_IsNullOrWhitespace(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() => Artist.Create(name!));

        Assert.Equal("name", ex.ParamName);
    }
}