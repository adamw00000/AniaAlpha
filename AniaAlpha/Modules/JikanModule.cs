using AniaAlpha.Models;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using JikanDotNet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AniaAlpha.Modules
{
    [Group("mal")]
    public class JikanModule: InteractiveBase
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
            var oldAccount = await GetAccount(Context.User, Context.Guild);
            if (oldAccount != null)
            {
                await ReplyAsync($"You have already registered an account: https://myanimelist.net/profile/{oldAccount.UserName}");
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

        [Command("unregister", RunMode = RunMode.Async)]
        public async Task Unregister()
        {
            var account = await GetAccount(Context.User, Context.Guild);
            if (account == null)
            {
                await ReplyAsync("You haven't registered an account yet!");
                return;
            }

            await ReplyAsync($"Are you sure you want to unregister that account: https://myanimelist.net/profile/{account.UserName}? (y/n)");
            var response = await NextMessageAsync(new YesNoCriterion());
            var content = response.Content.ToLower();

            if (content == "n" || content == "no")
            {
                return;
            }

            _context.MALAccounts.Remove(account);

            var modified = await _context.SaveChangesAsync();
            if (modified != 1)
            {
                await ReplyAsync("Database error.");
                return;
            }

            await ReplyAsync($"You have successfully unregistered your account.");
        }

        private async Task<MALAccount> GetAccount(SocketUser user, SocketGuild guild)
        {
            return await _context.MALAccounts.FirstOrDefaultAsync(mal => mal.UserId == user.Id && mal.GuildId == guild.Id);
        }

        [Command("get", RunMode = RunMode.Async)]
        public async Task Get()
        {
            await Get(Context.User);
        }

        [Command("get", RunMode = RunMode.Async)]
        public async Task Get(SocketUser user)
        {
            var account = await GetAccount(Context.User, Context.Guild);
            if (account == null)
            {
                await ReplyAsync("You haven't registered an account yet!");
                return;
            }

            await ReplyAsync($"https://myanimelist.net/profile/{account.UserName}");
        }
    }
}
