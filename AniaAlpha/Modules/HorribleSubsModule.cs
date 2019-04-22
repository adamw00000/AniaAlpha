using Discord.Commands;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AniaAlpha.Modules
{

    [Group("hs")]
    public class HorribleSubsModule: ModuleBase
    {
        const string baseUrl = @"https://horriblesubs.info";

        [Command("current", RunMode = RunMode.Async)]
        public async Task Current()
        {
            var url = baseUrl + "/current-season/";
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var currentShows = doc.DocumentNode
                .SelectNodes("//*[contains(@class, 'ind-show')]/a")
                .Select(node => new HsAnime
                {
                    Title = node.Attributes["title"].Value,
                    Url = baseUrl + node.Attributes["href"].Value
                });

            int i = 0;
            const int messageSize = 10;
            string aggregateBase = "**Current shows:**\n";
            while (true)
            {
                if (!currentShows.Any())
                    return;

                string response = currentShows.Take(messageSize)
                    .Aggregate(aggregateBase, (prev, next) => prev + $"{next.Title}: {next.Url}\n");
                await ReplyAsync(response);

                aggregateBase = "";
                i++;
                currentShows = currentShows.Skip(messageSize);
            }
        }

        [Command("track", RunMode = RunMode.Async)]
        public async Task Track([Remainder]string name)
        {
            var url = baseUrl + "/shows/";
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var trackedShowList = doc.DocumentNode
                .SelectNodes("//*[contains(@class, 'ind-show')]/a")
                .Select(node => new HsAnime
                {
                    Title = node.Attributes["title"].Value,
                    Url = baseUrl + node.Attributes["href"].Value
                })
                .Where(anime => anime.Title == name);

            if (!trackedShowList.Any())
            {
                await ReplyAsync("Anime of that name not found on HS!");
            }

            var trackedShow = trackedShowList.First();
            await ReplyAsync($"Tracking {trackedShow.Title}: {trackedShow.Url}!");
        }

        [Command("dl", RunMode = RunMode.Async)]
        public async Task Download([Remainder]string name = "")
        {
            var url = baseUrl + "/shows/";
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var trackedShowList = doc.DocumentNode
                .SelectNodes("//*[contains(@class, 'ind-show')]/a")
                .Select(node => new HsAnime
                {
                    Title = node.Attributes["title"].Value,
                    Url = baseUrl + node.Attributes["href"].Value
                })
                .Where(anime => anime.Title == name);

            if (!trackedShowList.Any())
            {
                await ReplyAsync("Anime of that name not found on HS!");
            }
            var trackedShow = trackedShowList.First();

            var dlDoc = await web.LoadFromWebAsync(trackedShow.Url);
            var nodes = doc.DocumentNode
                .SelectNodes("//*[contains(@class, 'dl-type hs-torrent-link')]/a");
            var dlLinks = doc.DocumentNode
                .SelectNodes("//*[contains(@class, 'dl-type hs-torrent-link')]/a")
                .Select(node => node.Attributes["href"].Value);

            string response = dlLinks.Aggregate("**Download links:**", (prev, next) => prev + $"{next}\n");
        }

        public class HsAnime
        {
            public string Title { get; set; }
            public string Url { get; set; }
        }
    }
}
