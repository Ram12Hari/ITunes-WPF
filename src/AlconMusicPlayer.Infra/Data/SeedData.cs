using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Infra.Data
{
    public static class SeedData
    {
        public static IReadOnlyList<Track> Build()
        {
            var artists = BuildArtists();
            var albums = BuildAlbums();
            var tracks = BuildTracks(artists, albums);
            return tracks.ToList();
        }

        private static Dictionary<string, Artist> BuildArtists() => new()
        {
            ["A.R. Rahman"] = Artist.Create("A.R. Rahman"),
            ["Ilaiyaraaja"] = Artist.Create("Ilaiyaraaja"),
            ["Yuvan Shankar Raja"] = Artist.Create("Yuvan Shankar Raja"),
            ["Pink Floyd"] = Artist.Create("Pink Floyd"),
            ["Coldplay"] = Artist.Create("Coldplay"),
            ["Radiohead"] = Artist.Create("Radiohead"),
        };

        private static Dictionary<string, Album> BuildAlbums() => new()
        {
            ["Roja"] = Album.Create("Roja"),
            ["Bombay"] = Album.Create("Bombay"),
            ["Mouna Ragam"] = Album.Create("Mouna Ragam"),
            ["7G Rainbow Colony"] = Album.Create("7G Rainbow Colony"),
            ["The Dark Side of the Moon"] = Album.Create("The Dark Side of the Moon"),
            ["A Rush of Blood to the Head"] = Album.Create("A Rush of Blood to the Head"),
            ["OK Computer"] = Album.Create("OK Computer"),
        };

        private static IReadOnlyList<Track> BuildTracks(Dictionary<string, Artist> a, Dictionary<string, Album> al) =>
        [

            // A.R. Rahman — Roja
        Track.Create("Roja Jaaneman",              "Roja/01-roja-jaaneman.mp3",           a["A.R. Rahman"], al["Roja"],                          Genre.Classical, 318),
        Track.Create("Dil Hai Chhota Sa",          "Roja/02-dil-hai-chhota-sa.mp3",       a["A.R. Rahman"], al["Roja"],                          Genre.Classical, 285),
        Track.Create("Chinna Chinna Aasai",        "Roja/03-chinna-chinna-aasai.mp3",     a["A.R. Rahman"], al["Roja"],                          Genre.Pop,       302),
        Track.Create("Rukmani Rukmani",            "Roja/04-rukmani-rukmani.mp3",         a["A.R. Rahman"], al["Roja"],                          Genre.Pop,       344),
        Track.Create("Ye Haseen Wadiya",           "Roja/05-ye-haseen-wadiya.mp3",        a["A.R. Rahman"], al["Roja"],                          Genre.Classical, 271),

        // A.R. Rahman — Bombay
        Track.Create("Kannalane",                  "Bombay/01-kannalane.mp3",             a["A.R. Rahman"], al["Bombay"],                        Genre.Classical, 336),
        Track.Create("Humma Humma",                "Bombay/02-humma-humma.mp3",           a["A.R. Rahman"], al["Bombay"],                        Genre.Pop,       358),
        Track.Create("Kehna Hi Kya",               "Bombay/03-kehna-hi-kya.mp3",          a["A.R. Rahman"], al["Bombay"],                        Genre.Classical, 312),
        Track.Create("Tu Hi Re",                   "Bombay/04-tu-hi-re.mp3",              a["A.R. Rahman"], al["Bombay"],                        Genre.Classical, 389),
        Track.Create("Uyire Uyire",                "Bombay/05-uyire-uyire.mp3",           a["A.R. Rahman"], al["Bombay"],                        Genre.Pop,       347),

        // Ilaiyaraaja — Mouna Ragam
        Track.Create("Ninaivellam Nithya",         "MounaRagam/01-ninaivellam.mp3",       a["Ilaiyaraaja"], al["Mouna Ragam"],                   Genre.Classical, 298),
        Track.Create("Chinna Kuyil Paadum",        "MounaRagam/02-chinna-kuyil.mp3",      a["Ilaiyaraaja"], al["Mouna Ragam"],                   Genre.Classical, 322),
        Track.Create("Pani Vizhum Malarvanam",     "MounaRagam/03-pani-vizhum.mp3",       a["Ilaiyaraaja"], al["Mouna Ragam"],                   Genre.Classical, 341),
        Track.Create("Mouna Ragam",                "MounaRagam/04-mouna-ragam.mp3",       a["Ilaiyaraaja"], al["Mouna Ragam"],                   Genre.Classical, 276),
        Track.Create("Andhi Mazhai",               "MounaRagam/05-andhi-mazhai.mp3",      a["Ilaiyaraaja"], al["Mouna Ragam"],                   Genre.Classical, 264),

        // Yuvan Shankar Raja — 7G Rainbow Colony
        Track.Create("Kannamoochi Yenada",         "7G/01-kannamoochi-yenada.mp3",        a["Yuvan Shankar Raja"], al["7G Rainbow Colony"],       Genre.Pop,       289),
        Track.Create("Yennai Arindhaal",           "7G/02-yennai-arindhaal.mp3",          a["Yuvan Shankar Raja"], al["7G Rainbow Colony"],       Genre.Pop,       312),
        Track.Create("Kadhal Yaanai",              "7G/03-kadhal-yaanai.mp3",             a["Yuvan Shankar Raja"], al["7G Rainbow Colony"],       Genre.Pop,       334),
        Track.Create("Venmathi Venmathi",          "7G/04-venmathi-venmathi.mp3",         a["Yuvan Shankar Raja"], al["7G Rainbow Colony"],       Genre.Ambient,   298),
        Track.Create("Thoda Thoda Mallikai",       "7G/05-thoda-thoda-mallikai.mp3",      a["Yuvan Shankar Raja"], al["7G Rainbow Colony"],       Genre.Pop,       267),

        // Pink Floyd — The Dark Side of the Moon
        Track.Create("Speak to Me",                "DSOTM/01-speak-to-me.mp3",            a["Pink Floyd"],  al["The Dark Side of the Moon"],     Genre.Rock,       68),
        Track.Create("Breathe",                    "DSOTM/02-breathe.mp3",                a["Pink Floyd"],  al["The Dark Side of the Moon"],     Genre.Rock,      169),
        Track.Create("Time",                       "DSOTM/03-time.mp3",                   a["Pink Floyd"],  al["The Dark Side of the Moon"],     Genre.Rock,      421),
        Track.Create("The Great Gig in the Sky",   "DSOTM/04-great-gig.mp3",              a["Pink Floyd"],  al["The Dark Side of the Moon"],     Genre.Rock,      284),
        Track.Create("Money",                      "DSOTM/05-money.mp3",                  a["Pink Floyd"],  al["The Dark Side of the Moon"],     Genre.Rock,      382),

        // Coldplay — A Rush of Blood to the Head
        Track.Create("Politik",                    "AROBTTH/01-politik.mp3",              a["Coldplay"],    al["A Rush of Blood to the Head"],   Genre.Rock,      321),
        Track.Create("In My Place",                "AROBTTH/02-in-my-place.mp3",          a["Coldplay"],    al["A Rush of Blood to the Head"],   Genre.Rock,      228),
        Track.Create("The Scientist",              "AROBTTH/03-the-scientist.mp3",        a["Coldplay"],    al["A Rush of Blood to the Head"],   Genre.Rock,      311),
        Track.Create("Clocks",                     "AROBTTH/04-clocks.mp3",              a["Coldplay"],    al["A Rush of Blood to the Head"],   Genre.Rock,      307),
        Track.Create("The Hardest Part",           "AROBTTH/05-the-hardest-part.mp3",    a["Coldplay"],    al["A Rush of Blood to the Head"],   Genre.Rock,      261),

        // Radiohead — OK Computer
        Track.Create("Airbag",                     "OKComputer/01-airbag.mp3",            a["Radiohead"],   al["OK Computer"],                   Genre.Rock,      277),
        Track.Create("Paranoid Android",           "OKComputer/02-paranoid-android.mp3",  a["Radiohead"],   al["OK Computer"],                   Genre.Rock,      387),
        Track.Create("Subterranean Homesick Alien","OKComputer/03-subterranean.mp3",      a["Radiohead"],   al["OK Computer"],                   Genre.Rock,      274),
        Track.Create("Exit Music (For a Film)",    "OKComputer/04-exit-music.mp3",        a["Radiohead"],   al["OK Computer"],                   Genre.Rock,      264),
        Track.Create("Let Down",                   "OKComputer/05-let-down.mp3",          a["Radiohead"],   al["OK Computer"],                   Genre.Rock,      299),
    ];
    }

}
