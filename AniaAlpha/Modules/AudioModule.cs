using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;

namespace AniaAlpha.Modules
{
    public class AudioModule : InteractiveBase
    {
        private LavaNode _node;
        private LavaPlayer _player;
        private readonly Lavalink _lavalink;
        private readonly DiscordSocketClient _client;

        public AudioModule(Lavalink lavalink, DiscordSocketClient client)
        {
            _lavalink = lavalink;
            _client = client;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            _node = _lavalink.DefaultNode;
            _player = _node.GetPlayer(Context.Guild.Id);

            _node.TrackFinished = async (player, track, reason) =>
            {
                if (player.Queue.Count != 0)
                    await player.PlayAsync(player.Queue.Dequeue());
            };

            base.BeforeExecute(command);
        }
        
        [Command("play", RunMode = RunMode.Async)]
        public async Task Play([Remainder]string name)
        {
            var channel = (Context.User as IGuildUser).VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("You are not connected to any voice channel!");
                return;
            }

            var result = await _node.SearchYouTubeAsync(name);
            if (result.Tracks.Count() == 0)
            {
                await ReplyAsync("Results not found!");
                return;
            }

            int resultSize = 5;
            int i = 1;
            var resultString = result.Tracks.Take(resultSize).Aggregate("**Choose the track to play:**\n", (prev, track) =>
            {
                return prev + "**" + i++ + ".** " + track.Title.ToString() + " (<" + track.Uri.ToString() + ">)\n";
            });

            var listMessage = await ReplyAsync(resultString);

            int number;
            SocketMessage response;
            while (true)
            {
                response = await NextMessageAsync(new NumberCriterion());
                number = int.Parse(response.Content);
                await (response as IUserMessage).DeleteAsync();
                if (number <= 0 || number > resultSize || number > result.Tracks.Count())
                {
                    await ReplyAsync("Invalid track number");
                }
                else
                    break;
            }

            _player = await _node.ConnectAsync(channel);

            var selectedTrack = result.Tracks.Skip(number - 1).FirstOrDefault();

            if (_player.IsPlaying)
            {
                _player.Queue.Enqueue(selectedTrack);
                await ReplyAsync($"Added track to the queue at position **{_player.Queue.Count}.**");
                await Queue();
            }
            else
                await _player.PlayAsync(selectedTrack);

            await listMessage.DeleteAsync();
        }
        
        [Command("queue")]
        public async Task Queue()
        {
            if (_player == null)
            {
                await ReplyAsync("Player not connected to any voice channel!");
                return;
            }
            if (_player.Queue.Count == 0)
            {
                await ReplyAsync("Queue is empty!");
                return;
            }

            int i = 1;
            var queueString = _player.Queue.Items.Aggregate("**Queue:**\n", (prev, track) =>
            {
                return prev + "**" + i++ + ".** " + track.Title.ToString() + "\n";
            });

            await ReplyAsync(queueString);
        }

        [Command("skip")]
        public async Task Skip(int n = 0)
        {
            if (_player == null)
            {
                await ReplyAsync("Player not connected to any voice channel!");
                return;
            }

            if (n == 0)
                await _player.SkipAsync();
            else
            {
                if (n > 0 && n <= _player.Queue.Count)
                    _player.Queue.RemoveAt(n - 1);
                else
                {
                    await ReplyAsync("Track at this position doesn't exist");
                    return;
                }
            }
            await ReplyAsync("Track skipped!");
            await Queue();
        }

        [Command("leave"), Alias(new string[] { "stop" })]
        public async Task Leave()
        {
            if (_player == null)
            {
                await ReplyAsync("Player not connected to any voice channel!");
                return;
            }
            await _node.DisconnectAsync(Context.Guild.Id);
            _player = _node.GetPlayer(Context.Guild.Id);
        }

        [Command("nowplaying")]
        public async Task NowPlaying()
        {
            if (_player == null)
            {
                await ReplyAsync("Player not connected to any voice channel!");
                return;
            }

            var track = _player.CurrentTrack;            
            if (track == null)
                await ReplyAsync("Currently not playing!");
            
            await ReplyAsync(track.Title.ToString() + " (" + track.Uri.ToString() + ")\n");
        }

        [Command("q"), Alias(new string[] { "quit" })]
        public async Task Exit()
        {
            await _client.StopAsync();
        }
    }
}
