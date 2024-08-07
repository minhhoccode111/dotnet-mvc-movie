// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc.Rendering;
using GameManagementMvc.Data;
using GameManagementMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GameManagementMvc.Controllers
{
    public class GameController : Controller
    {
        // logging
        private readonly ILogger<GameController> _logger;

        // db context
        private readonly GameManagementMvcContext _context;

        public GameController(ILogger<GameController> logger, GameManagementMvcContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Game
        public async Task<IActionResult> Index(
            string searchRating,
            string searchTitle,
            string searchGenre,
            string searchCompany,
            string sortBy
        )
        {
            // if Game model not exists in current context
            if (_context.Game == null)
            {
                // produce a problem detail response
                return Problem("Entity set 'GameManagementMvc.Game' is null.");
            }

            // use `var` to infer the type in case we change in the future
            // so that we don't have to change everywhere

            // select all game in current context and make as queryable
            var games = _context.Game.OrderByDescending(g => g.Rating).AsQueryable();

            games = GameSortBy(games, sortBy);

            // if search rating is provided then we try convert it to int
            // if success it will return true and a variable SearchRatingInt
            // type int to use
            if (int.TryParse(searchRating, out int searchRatingInt))
            {
                // filter games base on SearchRating
                games = games.Where(game => game.Rating == searchRatingInt);
            }

            // if search title is provided
            if (!String.IsNullOrEmpty(searchTitle))
            {
                // filter search title
                games = games.Where(game => game.Title!.ToUpper().Contains(searchTitle.ToUpper()));
            }

            // if search company is provided, and can be converted to int
            if (int.TryParse(searchCompany, out int searchCompanyInt))
            {
                // filter search company
                games = games.Where(game => game.CompanyId == searchCompanyInt);
            }

            // if search genre is provided, and can be converted to int
            if (int.TryParse(searchGenre, out int searchGenreInt))
            {
                // filter base on search genre
                games = games.Where(game => game.GenreIds!.Contains(searchGenreInt));
            }

            // make all games left a list
            var gameList = await games.ToListAsync();

            foreach (var game in gameList)
            {
                // populate Genres with GenreIds
                game.Genres = await PopulateGenreIdsInGame(game);
                // populate Company with CompanyId
                game.Company = await PopulateCompanyIdInGame(game);
            }

            // create a view mode to pass to game index view
            var gameVM = new GameViewModel
            {
                SearchCompany = searchCompany,
                SearchRating = searchRating,
                SearchGenre = searchGenre,
                SearchTitle = searchTitle, // default value of search input
                Companies = await GetContextCompaniesSelectList(),
                Genres = await GetContextGenresSelectList(),
                Games = gameList
            };

            return View(gameVM);
        }

        // GET: Game/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // if id not provided
            if (id == null)
            {
                return NotFound();
            }

            // find the game match the id in current context
            var game = await _context.Game.FirstOrDefaultAsync(m => m.Id == id);

            // if game not found
            if (game == null)
            {
                return NotFound();
            }

            // populate GenreIds field with real genres
            game.Genres = await PopulateGenreIdsInGame(game);
            // populate Company field with real CompanyId
            game.Company = await PopulateCompanyIdInGame(game);

            return View(game);
        }

        // GET: Game/Create
        public async Task<IActionResult> Create()
        {
            var genres = await GetContextGenresMultiSelectList();
            ViewData["Genres"] = genres;
            var companies = await GetContextCompaniesSelectList();
            ViewData["Companies"] = companies;
            return View();
        }

        // POST: Game/Create
        // To protect from overposting attacks, enable the specific properties you
        // want to bind to. For more details, see
        // http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        // only execute the method if anti-forgery-token pass
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Title,Body,Image,Rating,ReleaseDate,CompanyId,GenreIds")] Game game
        )
        {
            // in case race condition
            if (!IsValidCompanyId(game.CompanyId) || !IsValidGenreIds(game.GenreIds))
            {
                var genres = await GetContextGenresMultiSelectList();
                ViewData["Genres"] = genres;
                var companies = await GetContextCompaniesSelectList();
                ViewData["Companies"] = companies;
                return View("Create", game);
            }

            // if everything ok
            if (ModelState.IsValid)
            {
                // add that game to current context
                _context.Add(game);

                // save change
                await _context.SaveChangesAsync();

                // redirect to index
                return RedirectToAction(nameof(Index));
            }

            // else display view again with that game
            return View(game);
        }

        // GET: Game/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // if id not provided
            if (id == null)
            {
                return NotFound();
            }

            // find game with provided id in current context
            var game = await _context.Game.FirstOrDefaultAsync(m => m.Id == id);

            // if game not found
            if (game == null)
            {
                return NotFound();
            }

            // genres must be a <MultiSelectList>
            var genres = await GetContextGenresMultiSelectList(game.GenreIds!);
            // companies must be a <SelectList>
            var companies = await GetContextCompaniesSelectList(game.CompanyId);

            // pass to genres and companies to update
            ViewData["Genres"] = genres;
            ViewData["Companies"] = companies;

            return View(game);
        }

        // POST: Game/Edit/5
        // To protect from overposting attacks, enable the specific properties you
        // want to bind to. For more details, see
        // http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Body,Image,Rating,ReleaseDate,CompanyId,GenreIds")] Game game
        )
        {
            // mismatch between game model's id and route's id
            // in case race condition
            if (id != game.Id)
            {
                return NotFound();
            }

            if (!IsValidCompanyId(game.CompanyId) || !IsValidGenreIds(game.GenreIds))
            {
                return Redirect($"/Game/Edit/{game.Id}");
            }

            // form validation state
            if (ModelState.IsValid)
            {
                try
                {
                    // update game in current context
                    _context.Update(game);

                    // save change in current context
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // if current context error, probably race condition
                    if (!IsGameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // redirect to index
                return RedirectToAction(nameof(Index));
            }

            // render the view with the game model again
            return View(game);
        }

        // GET: Game/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // if id not provided
            if (id == null)
            {
                return NotFound();
            }

            // find the game match the id in current context
            var game = await _context.Game.FirstOrDefaultAsync(m => m.Id == id);

            // if game not found
            if (game == null)
            {
                return NotFound();
            }

            // populate Genres field using GenreIds field
            game.Genres = await PopulateGenreIdsInGame(game);
            // populate Company field using CompanyId field
            game.Company = await PopulateCompanyIdInGame(game);

            return View(game);
        }

        // POST: Game/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // find the game match the id in current context
            var game = await _context.Game.FindAsync(id);

            // if game found
            if (game != null)
            {
                // remove game from current context
                _context.Game.Remove(game);
            }

            // save change to current context
            await _context.SaveChangesAsync();

            // Redirects to the specified action using the actionName .
            return RedirectToAction(nameof(Index));
        }

        // ############################## HELPERS ##############################

        // use to sort games base on provided string
        private IQueryable<Game> GameSortBy(IQueryable<Game> games, string sortBy)
        {
            if (sortBy == "name")
                return games.OrderBy(g => g.Title);
            if (sortBy == "-name")
                return games.OrderByDescending(g => g.Title);
            if (sortBy == "date")
                return games.OrderBy(g => g.ReleaseDate);
            if (sortBy == "-date")
                return games.OrderByDescending(g => g.ReleaseDate);
            if (sortBy == "rating")
                return games.OrderBy(g => g.Rating);
            if (sortBy == "-rating")
                return games.OrderByDescending(g => g.Rating);
            return games;
        }

        // use to check if a game exists in current context
        private bool IsGameExists(int id)
        {
            // if any mode in current Game context has that id
            return _context.Game.Any(e => e.Id == id);
        }

        // use to check if a company's id is valid in create and edit action
        private bool IsValidCompanyId(int id)
        {
            // if any model in current Company context has that id
            return _context.Company.Any(c => c.Id == id);
        }

        // use to check a list of GenreIds are valid genres exist in current context
        // to use when create and edit games
        private bool IsValidGenreIds(List<int>? genreIds = null)
        {
            if (genreIds == null)
            {
                return false;
            }
            // select all genres, only take ids
            var genres = _context.Genre.Select(g => g.Id);
            // check if every id in genreIds arg is included in genres
            return genreIds.All(id => genres.Contains(id));
        }

        // use to populate Company field in game model base on CompanyId field
        private async Task<Company?> PopulateCompanyIdInGame(Game game)
        {
            // return the first one that match the id
            return await _context.Company.FirstOrDefaultAsync(c => c.Id == game.CompanyId);
        }

        // use to populate Genres field in game model base on GenreIds field
        private async Task<List<Genre>> PopulateGenreIdsInGame(Game game)
        {
            // return a list of genres in game's GenreIds
            return await _context
                .Genre.Where(genre => game.GenreIds!.Contains(genre.Id))
                .ToListAsync();
        }

        // use to generate select dropdown when filter games' company
        private async Task<SelectList> GetContextGenresSelectList(int? selected = null)
        {
            // select every genres' title in current context (to make a select list
            // filter)
            var genres = await _context
                .Genre.OrderBy(g => g.Title)
                // .Select(g => g.Title)
                // .DistinctBy(g => g.Title)
                .ToListAsync();

            // if selected is provided then it will be default selected
            return new SelectList(genres, "Id", "Title", selected?.ToString());
        }

        /*
           new SelectList(items, dataValueField, dataTextField, selectedValue)
           items: the list of items you want to display in SelectList
           dataValueField: the field will be used for <option>'s value
           dataTextField: the field will be used for <option>'s text display
           selectedValue: the default selected value, have to match one of the
           dataValueField
        */

        // use to generate select dropdown when create, edit and filter games' company
        private async Task<SelectList> GetContextCompaniesSelectList(int? selected = null)
        {
            // select every company' title in current context to make a select (filter,
            // edit, create)
            var companies = await _context
                .Company.OrderBy(c => c.Title)
                // .Select(c => c.Title)
                // .DistinctBy(g => g.Title)
                .ToListAsync();

            // WARN: selected MUST be a string for selected item to work and not
            // an int or a Company model
            return new SelectList(companies, "Id", "Title", selected?.ToString());
        }

        // use to generate checkboxes when create and edit games' genres
        private async Task<MultiSelectList> GetContextGenresMultiSelectList(
            List<int>? selected = null
        )
        {
            var genres = await _context.Genre.OrderBy(g => g.Title).ToListAsync();

            return new MultiSelectList(genres, "Id", "Title", selected);
        }
    }
}
