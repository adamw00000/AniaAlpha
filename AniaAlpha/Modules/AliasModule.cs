using Discord.Commands;
using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.Addons.Interactive;
using AniaAlpha.Models;

namespace AniaAlpha.Modules
{
    [Group("alias")]
    public class AliasModule: InteractiveBase
    {
        private readonly IJikan _jikan;
        private readonly DataContext _context;

        public AliasModule(IJikan jikan, DataContext context)
        {
            _jikan = jikan;
            _context = context;
        }

        [Command("Add", RunMode = RunMode.Async)]
        public async Task Add(string animeName, string alias)
        {
            if (_context.Aliases.Any(x => x.Name == alias))
            {
                await ReplyAsync("Alias already exists!");
                return;
            }

            var searchResult = (await _jikan.SearchAnime(animeName)).Results.Take(5).ToList();
            if (searchResult.Count == 0)
            {
                await ReplyAsync("No anime matching!");
                return;
            }

            var perfectMatch = searchResult.FirstOrDefault(result => result.Title == animeName);
            if (perfectMatch == null)
            {
                await ReplyAsync("No anime matching!");
                return;
            }

            var animeAlias = new AnimeAlias { AnimeId = perfectMatch.MalId, Name = alias };
            await _context.Aliases.AddAsync(animeAlias);

            var modified = await _context.SaveChangesAsync();

            if (modified != 1)
            {
                await ReplyAsync("Database error.");
                return;
            }

            await ReplyAsync($"Alias added!");
        }

        [Command("Add", RunMode = RunMode.Async)]
        public async Task Add(params string[] parameters)
        {
            if (!parameters[parameters.Length - 1].EndsWith('}'))
            {
                await ReplyAsync("Parsing error. Did you forget about {alias}?");
                return;
            }

            for (int i = 1; i < parameters.Length; i++)
            {
                if (parameters[i].StartsWith('{'))
                {
                    var animeName = parameters.Take(i).Aggregate((old, next) => old + " " + next);
                    var alias = parameters.Skip(i).Aggregate((old, next) => old + " " + next);
                    alias = alias.Remove(alias.Length - 1, 1).Remove(0, 1);

                    await Add(animeName, alias);
                }
            }
        }
    }
}
