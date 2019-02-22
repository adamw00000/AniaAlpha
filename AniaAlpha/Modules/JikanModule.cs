using AniaAlpha.Models;
using Discord.Commands;
using JikanDotNet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AniaAlpha.Modules
{
    [Group("mal")]
    public class JikanModule: ModuleBase
    {
        private readonly IJikan _jikan;
        private readonly DataContext _context;

        public JikanModule(IJikan jikan, DataContext context)
        {
            _jikan = jikan;
            _context = context;
        }

        [Command("register", RunMode = RunMode.Async)]
        public async Task Register(string username)
        {
            var oldMAL = await _context.MALAccounts.FirstOrDefaultAsync(mal => mal.UserId == Context.User.Id && mal.GuildId == Context.Guild.Id);
            if (oldMAL != null)
            {
                await ReplyAsync($"You have already registered an account: https://myanimelist.net/profile/{oldMAL.UserName}");
                return;
            }

            var profile = await _jikan.GetUserProfile(username);
            if (profile == null)
            {
                await ReplyAsync($"Account of that name doesn't exist");
                return;
            }

            MALAccount newAccount = new MALAccount { UserId = Context.User.Id, GuildId = Context.Guild.Id, UserName = username };
            await _context.MALAccounts.AddAsync(newAccount);

            var modified = await _context.SaveChangesAsync();
            if (modified != 1)
            {
                await ReplyAsync("Database error.");
                return;
            }

            await ReplyAsync($"An account has been registered: https://myanimelist.net/profile/{username}");
        }
    }
}
