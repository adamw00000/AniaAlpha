using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AniaAlpha
{
    class NumberCriterion : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            return Task.FromResult(int.TryParse(parameter.Content, out int n));
        }
    }

    class YesNoCriterion : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            bool result = false;

            string[] acceptedValues = { "y", "yes", "n", "no" };
            var content = parameter.Content.ToLower();

            if (acceptedValues.Contains(content))
            {
                result = true;
            }

            return Task.FromResult(result);
        }
    }
}
