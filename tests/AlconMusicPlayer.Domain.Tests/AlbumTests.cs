using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Domain.Tests;

public class AlbumTests
{
    [Fact]
    public void Create_Should_Return_Album_With_Given_Name()
    {
        var album = Album.Create("OK Computer");

        Assert.NotNull(album);
        Assert.Equal("OK Computer", album.Name);
    }

    [Fact]
    public void Create_Should_Assign_New_Non_Empty_Guid()
    {
        var album = Album.Create("Roja");

        Assert.NotEqual(Guid.Empty, album.Id);
    }

    [Fact]
    public void Create_Should_Generate_Unique_Ids()
    {
        var a1 = Album.Create("A1");
        var a2 = Album.Create("A2");

        Assert.NotEqual(a1.Id, a2.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_Name_IsNullOrWhitespace(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() => Album.Create(name!));

        Assert.Equal("name", ex.ParamName);
    }
}