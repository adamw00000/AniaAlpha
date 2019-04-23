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
                .SelectNodes("//*[contains(@class, 'ind-show')]" +
                    "/a")
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
                .SelectNodes("//*[contains(@class, 'ind-show')]" +
                    "/a")
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
                .SelectNodes("//*[contains(@class, 'ind-show')]" +
                    "/a")
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

            var linkList = await GetDls(trackedShow.Url, name);
            if (linkList == null)
            {
                await ReplyAsync("No (or incomplete) download links available!");
            }

            //var responseLines = linkList.Split('\n').AsEnumerable();

            const int messageSize = 12;
            int weight = 0;
            string nextResponse = "**Download links:**\n";
            foreach (var link in linkList)
            {
                if (weight + link.LinkWeight > messageSize)
                {
                    await ReplyAsync(nextResponse);
                    weight = 0;
                    nextResponse = "";
                }

                string responseFragment;
                if (link.Type == LinkType.Torrent)
                    responseFragment = $"{link.AnimeTitle} {link.Episode} - {link.Url}\n";
                else
                    responseFragment = $"{link.AnimeTitle} {link.Episode} - magnet ```{link.Url}```\n";
                nextResponse += responseFragment;
                weight += link.LinkWeight;
            }

            await ReplyAsync(nextResponse);
        }

        public async Task<IEnumerable<HsLink>> GetDls(string url, string animeName)
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
                var moreButton = await page.XPathAsync($"//*[contains(@class, 'more-button') and @id='{i}']");
                if (!moreButton.Any())
                    break;
                await moreButton.FirstOrDefault()?.ClickAsync();
                i++;
            }

            var matches = await page.XPathAsync($"//*[contains(@class, 'rls-label')]");
            matches.ToList()
                .ForEach(async match => await match.ClickAsync());

            var episodeDivs = await page
                .XPathAsync($"//*[contains(@class, 'rls-info-container')]");

            List<HsLink> links = new List<HsLink>();
            foreach (var episodeDiv in episodeDivs)
            {
                string episodeNumber = await (await episodeDiv.GetPropertyAsync("id")).JsonValueAsync<string>();

                LinkType type = LinkType.Torrent;
                var linkSpans = await page
                    .XPathAsync($"//*[contains(@class, 'rls-info-container') and @id='{episodeNumber}']" +
                       $"/*[contains(@class, 'rls-links-container')]" +
                       $"/*[contains(@class, 'rls-link link-720p')]" +
                       $"/*[contains(@class, 'dl-type hs-torrent-link')]" +
                       $"/a");

                if (!linkSpans.Any())
                {
                    type = LinkType.Magnet;
                    linkSpans = await page
                        .XPathAsync($"//*[contains(@class, 'rls-info-container') and @id='{episodeNumber}']" +
                           $"/*[contains(@class, 'rls-links-container')]" +
                           $"/*[contains(@class, 'rls-link link-720p')]" +
                           $"/*[contains(@class, 'dl-type hs-magnet-link')]" +
                           $"/a");
                    if (!linkSpans.Any())
                    {
                        return null;
                    }
                }
                var linkSpan = linkSpans.First();

                string link = await (await linkSpan.GetPropertyAsync("href")).JsonValueAsync<string>();
                links.Add(new HsLink
                {
                    AnimeTitle = animeName,
                    Episode = episodeNumber,
                    Url = link,
                    Type = type
                });
            }

            return links;//.Aggregate("", (prev, next) => prev + $"{next.AnimeTitle} {next.Episode} - {next.Url}\n");
        }

        public class HsAnime
        {
            public string Title { get; set; }
            public string Url { get; set; }
        }

        public class HsLink
        {
            public string AnimeTitle { get; set; }
            public string Episode { get; set; }
            public string Url { get; set; }
            public LinkType Type { get; set; }
            public int LinkWeight => Type == LinkType.Torrent ? 1 : 4;
        }
        public enum LinkType { Torrent, Magnet };
    }
}
