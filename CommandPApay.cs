using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaupUconomyEssentials
{
    class CommandPApay
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "papay";

        public string Help => "Allows an allowed person to pay someone else not using their own premium currency.";

        public string Syntax => "<player name or id> <amt>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] msg)
        {
            var playerid = (UnturnedPlayer)caller;
            string message;
            if (msg.Length == 0)
            {
                message = UconomyEssentials.Instance.Translate("papay_usage_msg");
                // We are going to print how to use
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            if (msg.Length != 2)
            {
                message = UconomyEssentials.Instance.Translate("papay_usage_msg");
                // Print how to use
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            var rp = UnturnedPlayer.FromName(msg[0]);
            if (rp == null)
            {
                ulong.TryParse(msg[0], out var id);
                if (!((CSteamID)id).IsValid())
                {
                    message = UconomyEssentials.Instance.Translate("not_valid_player_msg", msg[0]);
                    UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                    return;
                }

                rp = UnturnedPlayer.FromCSteamID((CSteamID)id);
            }

            uint.TryParse(msg[1], out var amt);
            if (amt <= 0)
            {
                message = UconomyEssentials.Instance.Translate("not_valid_amount", msg[1]);
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            var newbal = Uconomy.Instance.Database.IncreasePremiumBalance(rp.CSteamID.ToString(), amt);
            UnturnedChat.Say(rp.CSteamID,
                UconomyEssentials.Instance.Translate("apaid_msg", playerid.CharacterName, amt,
                    Uconomy.Instance.Configuration.Instance.PremiumMoneyName, newbal,
                    Uconomy.Instance.Configuration.Instance.PremiumMoneyName), "https://i.imgur.com/6EoLIWM.png");
            UnturnedChat.Say(playerid,
                UconomyEssentials.Instance.Translate("apay_msg", rp.CharacterName, amt,
                    Uconomy.Instance.Configuration.Instance.PremiumMoneyName), "https://i.imgur.com/6EoLIWM.png");
            UconomyEssentials.HandleEvent(rp, amt, "ppaid");
        }
    }
}
