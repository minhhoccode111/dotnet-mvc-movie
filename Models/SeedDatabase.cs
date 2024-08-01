// using System;
// using System.Linq;
// using Microsoft.Extensions.DependencyInjection;
using GameManagementMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GameManagementMvc.Models
{
    public static class SeedDatabase
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (
                var context = new GameManagementMvcContext(
                    serviceProvider.GetRequiredService<DbContextOptions<GameManagementMvcContext>>()
                )
            )
            {
                // if any movie exists
                if (context.Game.Any() || context.Company.Any() || context.Genre.Any())
                {
                    // turn on to clear database

                    // context.Game.RemoveRange(context.Game);
                    // context.Company.RemoveRange(context.Company);
                    // context.Genre.RemoveRange(context.Genre);
                    // context.SaveChanges();
                    // Console.WriteLine("Database clear!");

                    return;
                }

                var ran = new Random();

                // if no movie exists
                var companyList = new Company[]
                {
                    // MAANG
                    new Company
                    {
                        Title = "Meta",
                        Body = "Meta is a very great company",
                        Image = "https://images.cnbctv18.com/wp-content/uploads/2022/09/Meta.jpg",
                        FoundingDate = DateTime.Parse(
                            $"{ran.Next(1990, 2024)}-{ran.Next(1, 12)}-{ran.Next(1, 30)}"
                        ),
                    },
                    new Company
                    {
                        Title = "Apple",
                        Body = "Apple is a very great company",
                        Image =
                            "https://dm0qx8t0i9gc9.cloudfront.net/thumbnails/video/UD7CEz6/editorial-apple-inc-logo-on-glass-building_smk22zqcg_thumbnail-1080_01.png",
                        FoundingDate = DateTime.Parse(
                            $"{ran.Next(1990, 2024)}-{ran.Next(1, 12)}-{ran.Next(1, 30)}"
                        ),
                    },
                    new Company
                    {
                        Title = "Amazon",
                        Body = "Amazon is a very great company",
                        Image =
                            "https://www.wealthandfinance-news.com/wp-content/uploads/2020/01/amazon.jpg",
                        FoundingDate = DateTime.Parse(
                            $"{ran.Next(1990, 2024)}-{ran.Next(1, 12)}-{ran.Next(1, 30)}"
                        ),
                    },
                    new Company
                    {
                        Title = "Netflix",
                        Body = "Netflix is a very great company",
                        Image =
                            "https://s.aolcdn.com/hss/storage/midas/dae3c205f61d252afbea973ef0409803/206200911/Netflix+Media_0193+2.jpg",
                        FoundingDate = DateTime.Parse(
                            $"{ran.Next(1990, 2024)}-{ran.Next(1, 12)}-{ran.Next(1, 30)}"
                        ),
                    },
                    new Company
                    {
                        Title = "Google",
                        Body = "Google is a very great company",
                        Image = "https://wallpapercave.com/wp/kmmXJbb.jpg",
                        FoundingDate = DateTime.Parse(
                            $"{ran.Next(1990, 2024)}-{ran.Next(1, 12)}-{ran.Next(1, 30)}"
                        ),
                    }
                };

                context.Company.AddRange(companyList);
                context.SaveChanges();
                var companies = context.Company.ToList();

                var genreList = new Genre[]
                {
                    new Genre { Title = "Action", Body = "Action is a very great genre" },
                    new Genre { Title = "Sport", Body = "Sport is a very great genre" },
                    new Genre { Title = "Horror", Body = "Horror is a very great genre" },
                    new Genre { Title = "Humor", Body = "Humor is a very great genre" },
                    new Genre { Title = "Romantic", Body = "Romantic is a very great genre" },
                    new Genre { Title = "FPS", Body = "FPS is a very great genre" },
                };

                // NOTE: have to do like this because we have to make the genres' Ids
                // available to save in GenreIds field in Game Models
                context.Genre.AddRange(genreList);
                context.SaveChanges();
                var genres = context.Genre.ToList();

                var numGames = 20;
                var games = new Game[numGames];
                for (int i = 0; i < numGames; i++)
                {
                    var genreIds = new List<int>();
                    var genre0 = ran.Next(0, genres.Count);
                    var genre1 = ran.Next(0, genres.Count);
                    var genre2 = ran.Next(0, genres.Count);
                    genreIds.Add(genres[genre0].Id);
                    genreIds.Add(genres[genre1].Id);
                    genreIds.Add(genres[genre2].Id);
                    Company company = companies[ran.Next(0, companies.Count)];
                    Game currGame = new Game
                    {
                        Title = $"Game {i}",
                        Body = $"Game {i} is a very great game",
                        // Image= "",
                        Rating = ran.Next(1, 6),
                        ReleaseDate = DateTime.Parse(
                            $"{ran.Next(1990, 2024)}-{ran.Next(1, 12)}-{ran.Next(1, 30)}"
                        ),
                        Company = company,
                        CompanyId = company.Id,
                        GenreIds = genreIds,
                    };
                    games[i] = currGame;
                }

                context.Game.AddRange(games);

                // save changes make to _context
                context.SaveChanges();
            }
        }
    }
}
