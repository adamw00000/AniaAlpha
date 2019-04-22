using Discord.Commands;
using HtmlAgilityPack;
using PuppeteerSharp;
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

            var responseString = await GetNextDl(trackedShow.Url);
            var responseLines = responseString.Split('\n').AsEnumerable();

            int i = 0;
            const int messageSize = 10;
            string aggregateBase = "**Download links:**\n";
            while (true)
            {
                if (!responseLines.Any())
                    return;

                string response = responseLines.Take(messageSize)
                    .Aggregate(aggregateBase, (prev, next) => prev + $"{next}\n");
                await ReplyAsync(response);

                aggregateBase = "";
                i++;
                responseLines = responseLines.Skip(messageSize);
            }

            //var dlDoc = await web.LoadFromWebAsync(trackedShow.Url);
            //var nodes = doc.DocumentNode
            //    .SelectNodes("//*[contains(@class, 'dl-type hs-torrent-link')]/a");
            //var dlLinks = doc.DocumentNode
            //    .SelectNodes("//*[contains(@class, 'dl-type hs-torrent-link')]/a")
            //    .Select(node => node.Attributes["href"].Value);

            //string response = dlLinks.Aggregate("**Download links:**", (prev, next) => prev + $"{next}\n");
        }

        public async Task<string> GetNextDl(string url)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);

            int i = 0;
            while (true)
            {
                var moreButton = await page.XPathAsync($"//*[contains(@class, 'more-button') and @id=\"{i}\"]");
                if (!moreButton.Any())
                    break;
                await moreButton.FirstOrDefault()?.ClickAsync();
                i++;
            }

            var matches = await page.XPathAsync($"//*[contains(@class, 'rls-label')]");
            matches.ToList()
                .ForEach(async match => await match.ClickAsync());

            var linkSpans = await page
                .XPathAsync($"//*[contains(@class, 'rls-link link-720p')]/span[contains(@class, 'dl-type hs-torrent-link')]/a");

            List<string> links = new List<string>();
            foreach(var span in linkSpans)
            {
                string link = await (await span.GetPropertyAsync("href")).JsonValueAsync<string>();
                links.Add(link);
            }
            return links.Aggregate("", (prev, next) => prev + $"{next}\n");
        }

        public class HsAnime
        {
            public string Title { get; set; }
            public string Url { get; set; }
        }
    }
}
