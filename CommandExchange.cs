using System.Collections.Generic;
using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace ZaupUconomyEssentials
{
    public class CommandExchange : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "exchange";

        public string Help => "Exchanges experience for economy currency.";

        public string Syntax => "<amount> [money]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] amt)
        {
            var playerid = (UnturnedPlayer) caller;
            string message;
            if (amt.Length == 0)
            {
                message = UconomyEssentials.Instance.Translate("exchange_usage_msg");
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            if (amt.Length > 2)
            {
                message = UconomyEssentials.Instance.Translate("exchange_usage_msg");
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            if (!UconomyEssentials.Instance.Configuration.Instance.ExpExchange && amt.Length == 1)
            {
                message = UconomyEssentials.Instance.Translate("experience_exchange_not_available");
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            if (!UconomyEssentials.Instance.Configuration.Instance.MoneyExchange && amt.Length == 2)
            {
                message = UconomyEssentials.Instance.Translate("money_exchange_not_available");
                UnturnedChat.Say(playerid, message);
                return;
            }

            // Get expereience balance first
            var exp = playerid.Experience;
            uint.TryParse(amt[0], out var examt);
            if (examt <= 0)
            {
                message = UconomyEssentials.Instance.Translate("exchange_zero_amount_error");
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            if (exp < examt && amt.Length == 1)
            {
                message = UconomyEssentials.Instance.Translate("exchange_insufficient_experience", examt.ToString());
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            // Get balance first
            var bal = Uconomy.Instance.Database.GetBalance(playerid.CSteamID.ToString());
            if (amt.Length > 1 && bal < examt)
            {
                message = UconomyEssentials.Instance.Translate("exchange_insufficient_money", examt.ToString(),
                    Uconomy.Instance.Configuration.Instance.MoneyName);
                UnturnedChat.Say(playerid, message, "https://i.imgur.com/FeIvao9.png");
                return;
            }

            switch (amt.Length)
            {
                case 1:
                    var gain = (decimal) (examt *
                                          UconomyEssentials.Instance.Configuration.Instance.ExpExchangerate);
                    // Just to make sure to avoid any errors
                    gain = decimal.Round(gain, 2);
                    var newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID.ToString(), gain);
                    message = UconomyEssentials.Instance.Translate("new_balance_msg", newbal,
                        Uconomy.Instance.Configuration.Instance.MoneyName);
                    UnturnedChat.Say(playerid, message, "https://i.imgur.com/6EoLIWM.png");
                    playerid.Experience -= examt;
                    UconomyEssentials.HandleEvent(playerid, gain, "exchange", examt);
                    Uconomy.Instance.Database.AddHistory(playerid.CSteamID, "UconomyEssentials", gain, "Exchange " + examt.ToString() + " EXP");
                    break;
                case 2:
                    var gainm = (uint) (examt *
                                        UconomyEssentials.Instance.Configuration.Instance.MoneyExchangerate);
                    // Just to make sure to avoid any errors
                    newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID.ToString(), examt * -1.0m);
                    message = UconomyEssentials.Instance.Translate("new_balance_msg", newbal,
                        Uconomy.Instance.Configuration.Instance.MoneyName);
                    UnturnedChat.Say(playerid, message, "https://i.imgur.com/6EoLIWM.png");
                    playerid.Experience += gainm;
                    UconomyEssentials.HandleEvent(playerid, gainm, "exchange", examt, "money");
                    Uconomy.Instance.Database.AddHistory(playerid.CSteamID, "UconomyEssentials", -examt, "Exchange " + gainm.ToString() + " Money");
                    break;
            }

            playerid.Player.channel.send("tellExperience", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER,
                playerid.Experience);
        }
    }
}