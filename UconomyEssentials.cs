using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;

namespace ZaupUconomyEssentials
{
    public class UconomyEssentials : RocketPlugin<UconomyEConfiguration>
    {
        public static UconomyEssentials Instance;
        public Dictionary<string, decimal> groups = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> premiumgroups = new Dictionary<string, decimal>();
        protected override void Load()
        {
            Instance = this;
            var nlgroup = Configuration.Instance.PayGroups.Distinct(new GroupComparer()).ToList();
            var nlpremiumgroup = Configuration.Instance.PayPremiumGroups.Distinct(new PremiumGroupComparer()).ToList();
            Configuration.Instance.PayGroups = nlgroup;
            foreach (var g in Configuration.Instance.PayGroups)
                try
                {
                    groups.Add(g.GroupID, g.Salary);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            foreach (var g in Configuration.Instance.PayPremiumGroups)
                try
                {
                    premiumgroups.Add(g.GroupID, g.Salary);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
        }

        protected override void Unload()
        {
            Instance.groups.Clear();
            Instance.premiumgroups.Clear();
        }

        public delegate void PlayerPaidEvent(UnturnedPlayer player, decimal amount);

        public event PlayerPaidEvent OnPlayerPaid;

        public delegate void PlayerPremiumPaidEvent(UnturnedPlayer player, decimal amount);

        public event PlayerPremiumPaidEvent OnPlayerPremiumPaid;

        public delegate void PlayerLossEvent(UnturnedPlayer player, decimal amount);

        public event PlayerLossEvent OnPlayerLoss;

        public delegate void PlayerExchangeEvent(UnturnedPlayer player, decimal currency, uint experience, string type);

        public event PlayerExchangeEvent OnPlayerExchange;

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {
                    "pay_time_msg",
                    "You have received {0} {1} in salary for being a {2}."
                },
                {
                    "unable_to_pay_group_msg",
                    "Unable to pay {0} as no {1} group salary set."
                },
                {
                    "to_killer_msg",
                    "You have received {0} {1} for killing {2}."
                },
                {
                    "lose_suicide_msg",
                    "You have had {0} {1} deducted from your account for committing suicide."
                },
                {
                    "new_balance_msg",
                    "Your new balance is {0} {1}."
                },
                {
                    "lose_money_on_death_msg",
                    "You have lost {0} {1} for being killed."
                },
                {
                    "apay_msg",
                    "You have paid {0} {1} {2}."
                },
                {
                    "apaid_msg",
                    "{0} gave you {1} {2}. You now have {3} {4}."
                },
                {
                    "experience_exchange_not_available",
                    "I'm sorry, experience exchange is not available.  Ask your admin to enable it."
                },
                {
                    "money_exchange_not_available",
                    "I'm sorry, money exchange is not available.  Ask your admin to enable it."
                },
                {
                    "exchange_zero_amount_error",
                    "You have to enter an amount of experience/money to exchange."
                },
                {
                    "exchange_insufficient_experience",
                    "You don't have {0} exprience."
                },
                {
                    "exchange_insufficent_money",
                    "You don't have {0} {1}."
                },
                {
                    "apay_usage_msg",
                    "Usage: /apay <player name or id> <amt>"
                },
                {
                    "apay_usage_msg",
                    "Usage: /papay <player name or id> <amt>"
                },
                {
                    "exchange_usage_msg",
                    "Usage: /exchange <amount>[ money] (including money will exchange money to xp otherwise defaults xp to money)"
                },
                {
                    "not_valid_player_msg",
                    "{0} is not a valid player name or steam id."
                },
                {
                    "not_valid_amount",
                    "{0} is not a correct amount."
                },
                {
                    "zombie_kill_paid_msg",
                    "You have been paid {0} {1} for killing a zombie.  Your balance is now {2} {3}."
                },
                {
                    "mega_zombie_kill_paid_msg",
                    "You have been paid {0} {1} for killing a mega zombie.  Your balance is now {2} {3}."
                }
            };

        public static void HandleEvent(UnturnedPlayer player, decimal amount, string eventtype, uint exp = 0,
            string type = "experience")
        {
            if (eventtype == "exchange" && exp <= 0)
            {
                Logger.Log("OnPlayerExchange could not be fired as no experience was given.");
                return;
            }

            if (amount == 0.0m || player == null)
            {
                Logger.Log(eventtype + " was not fired as player or amount not given.");
                return;
            }

            switch (eventtype)
            {
                case "paid":
                    Instance.OnPlayerPaid?.Invoke(player, amount);
                    player.Player.gameObject.SendMessage("UEOnPlayerPaid", new object[] {player, amount});
                    break;
                case "ppaid":
                    Instance.OnPlayerPremiumPaid?.Invoke(player, amount);
                    player.Player.gameObject.SendMessage("UEOnPlayerPremiumPaid", new object[] { player, amount });
                    break;
                case "loss":
                    Instance.OnPlayerLoss?.Invoke(player, amount);
                    player.Player.gameObject.SendMessage("UEOnPlayerLoss", new object[] {player, amount});
                    break;
                case "exchange":
                    Instance.OnPlayerExchange?.Invoke(player, amount, exp, type);
                    player.Player.gameObject.SendMessage("UEOnPlayerExchange",
                        new object[] {player, amount, exp, type});
                    break;
                default:
                    Logger.Log("Invalid type supplied to throw paid/loss/experience event.");
                    break;
            }
        }
    }
}