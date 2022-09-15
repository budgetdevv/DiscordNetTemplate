using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordNetTemplate.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule: ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task Test()
        {

        }
    }
}
