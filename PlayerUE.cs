using System;
using fr34kyn01535.Uconomy;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace ZaupUconomyEssentials
{
    public class PlayerUe : UnturnedPlayerComponent
    {
        private DateTime _lastpaid;
        private UnturnedPlayerEvents _rpe;

        protected void Start()
        {
            _rpe = gameObject.transform.GetComponent<UnturnedPlayerEvents>();
            _lastpaid = DateTime.Now;
            _rpe.OnDeath += Rpe_OnPlayerDeath;
            _rpe.OnUpdateStat += Rpe_OnUpdateStat;
        }

        private static void Rpe_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (player == null) return;
            if (murderer == CSteamID.Nil) return;

            var killer = UnturnedPlayer.FromCSteamID(murderer);
            if (killer == null) return;

            if (!UconomyEssentials.Instance.Configuration.Instance.PayHit) return;

            var u = Uconomy.Instance;
            var bal = u.Database.GetBalance(player.CSteamID.ToString());

            if (cause == EDeathCause.SUICIDE && UconomyEssentials.Instance.Configuration.Instance.LoseSuicide)
            {
                // We are going to remove currency for the suicide
                var loss = (decimal) UconomyEssentials.Instance.Configuration.Instance.LoseSuicideAmt * -1.0m;
                if (bal + loss < 0.0m) loss = bal * -1.0m;
                var bal1 = u.Database.IncreaseBalance(player.CSteamID.ToString(), loss);
                UconomyEssentials.HandleEvent(player, loss * -1.0m, "loss");
                UnturnedChat.Say(player.CSteamID,
                    UconomyEssentials.Instance.Translate("lose_suicide_msg",
                        UconomyEssentials.Instance.Configuration.Instance.LoseSuicideAmt,
                        u.Configuration.Instance.MoneyName), "https://i.imgur.com/abxMnf2.png");
                if (bal1 != 0m)
                    UnturnedChat.Say(player.CSteamID,
                        UconomyEssentials.Instance.Translate("new_balance_msg", bal1,
                            u.Configuration.Instance.MoneyName), "https://i.imgur.com/hOhFr7X.png");
                return;
            }

            // We do nothing if they suicide.
            if (cause == EDeathCause.SUICIDE &&
                !UconomyEssentials.Instance.Configuration.Instance.LoseSuicide)
                return;

            if (UconomyEssentials.Instance.Configuration.Instance.LoseMoneyOnDeath)
            {
                var loss = (decimal) UconomyEssentials.Instance.Configuration.Instance.LoseMoneyOnDeathAmt * -1.0m;
                if (bal + loss < 0.0m) loss = bal * -1.0m;
                u.Database.IncreaseBalance(player.CSteamID.ToString(), loss);
                UconomyEssentials.HandleEvent(player, loss * -1.0m, "loss");
                UnturnedChat.Say(player.CSteamID,
                    UconomyEssentials.Instance.Translate("lose_money_on_death_msg",
                        UconomyEssentials.Instance.Configuration.Instance.LoseMoneyOnDeathAmt,
                        u.Configuration.Instance.MoneyName), "https://i.imgur.com/abxMnf2.png");
            }

            // Pay the other player for the kill
            var balk = u.Database.IncreaseBalance(murderer.ToString(),
                (decimal) UconomyEssentials.Instance.Configuration.Instance.PayHitAmt);
            UconomyEssentials.HandleEvent(player,
                (decimal) UconomyEssentials.Instance.Configuration.Instance.PayHitAmt, "paid");
            if (!UconomyEssentials.Instance.Configuration.Instance.SendPayHitMsg)
                return; // No message is to be sent, so job is done.

            UnturnedChat.Say(murderer,
                UconomyEssentials.Instance.Translate("to_killer_msg",
                    UconomyEssentials.Instance.Configuration.Instance.PayHitAmt, u.Configuration.Instance.MoneyName,
                    player.CharacterName), "https://i.imgur.com/7DbFgKZ.png");
            if (bal != 0m)
                UnturnedChat.Say(murderer,
                    UconomyEssentials.Instance.Translate("new_balance_msg", balk, u.Configuration.Instance.MoneyName), "https://i.imgur.com/hOhFr7X.png");
        }

        private static void Rpe_OnUpdateStat(UnturnedPlayer player, EPlayerStat stat)
        {
            if (player == null) return;

            var u = Uconomy.Instance;
            if (UconomyEssentials.Instance.Configuration.Instance.PayZombie &&
                stat == EPlayerStat.KILLS_ZOMBIES_NORMAL)
            {
                var balzk = u.Database.IncreaseBalance(player.CSteamID.ToString(),
                    (decimal) UconomyEssentials.Instance.Configuration.Instance.PayZombieAmt);
                UconomyEssentials.HandleEvent(player,
                    (decimal) UconomyEssentials.Instance.Configuration.Instance.PayZombieAmt, "paid");
                if (!UconomyEssentials.Instance.Configuration.Instance.SendPayZombieMsg)
                    return; // No message is to be sent, so job is done.

                UnturnedChat.Say(player.CSteamID,
                    UconomyEssentials.Instance.Translate("zombie_kill_paid_msg",
                        UconomyEssentials.Instance.Configuration.Instance.PayZombieAmt,
                        u.Configuration.Instance.MoneyName, balzk, u.Configuration.Instance.MoneyName), "https://i.imgur.com/7DbFgKZ.png");
            }
            else if (UconomyEssentials.Instance.Configuration.Instance.PayMegaZombie &&
                     stat == EPlayerStat.KILLS_ZOMBIES_MEGA)
            {
                var balzk = u.Database.IncreaseBalance(player.CSteamID.ToString(),
                    (decimal) UconomyEssentials.Instance.Configuration.Instance.PayMegaZombieAmt);
                UconomyEssentials.HandleEvent(player,
                    (decimal) UconomyEssentials.Instance.Configuration.Instance.PayMegaZombieAmt, "paid");
                if (!UconomyEssentials.Instance.Configuration.Instance.SendPayMegaZombieMsg)
                    return; // No message is to be sent, so job is done.

                UnturnedChat.Say(player.CSteamID,
                    UconomyEssentials.Instance.Translate("mega_zombie_kill_paid_msg",
                        UconomyEssentials.Instance.Configuration.Instance.PayMegaZombieAmt,
                        u.Configuration.Instance.MoneyName, balzk, u.Configuration.Instance.MoneyName), "https://i.imgur.com/7DbFgKZ.png");
            }
        }

        private void FixedUpdate()
        {
            if (!UconomyEssentials.Instance.Configuration.Instance.PayTime ||
                !((DateTime.Now - _lastpaid).TotalSeconds >=
                  UconomyEssentials.Instance.Configuration.Instance.PayTimeSeconds)) return;

            _lastpaid = DateTime.Now;
            UconomyEssentials.Instance.groups.TryGetValue("all", out var pay);
            UconomyEssentials.Instance.premiumgroups.TryGetValue("all", out var ppay);
            var paygroup = "Player";
            var paypremiumgroup = "Player";
            var pass = false;
            var ppass = false;
            if (pay == 0.0m)
            {
                // We are checking for the different groups as All is not set.
                if (Player.IsAdmin && UconomyEssentials.Instance.groups.ContainsKey("admin"))
                {
                    UconomyEssentials.Instance.groups.TryGetValue("admin", out pay);
                    paygroup = "admin";
                    if (pay == 0.0m)
                    {
                        /*Logger.Log(UconomyEssentials.Instance.Translate(
                            "unable_to_pay_group_msg", Player.CharacterName, "admin"));*/
                        pass = true;
                    }
                }
                else
                {
                    // They aren't admin so we'll just go through like groups like normal.

                    var plgroups = R.Permissions.GetGroups(Player, true);
                    foreach (var s in plgroups)
                    {
                        //Logger.Log(s.Id);
                        UconomyEssentials.Instance.groups.TryGetValue(s.Id, out var pay2);
                        //Logger.Log(pay2.ToString());
                        if (pay2 <= pay) continue;

                        pay = pay2;
                        paygroup = s.Id;
                    }

                    if (pay == 0.0m)
                    {
                        // We assume they are default group.
                        UconomyEssentials.Instance.groups.TryGetValue("default", out pay);
                        if (pay == 0.0m)
                        {
                            // There was an error.  End it.
                            /*Logger.Log(
                                UconomyEssentials.Instance.Translate("unable_to_pay_group_msg", Player.CharacterName,
                                    ""));*/
                            pass = true;
                        }
                    }
                }
            }

            if (ppay == 0.0m)
            {
                // We are checking for the different groups as All is not set.
                if (Player.IsAdmin && UconomyEssentials.Instance.premiumgroups.ContainsKey("admin"))
                {
                    UconomyEssentials.Instance.premiumgroups.TryGetValue("admin", out ppay);
                    paypremiumgroup = "admin";
                    if (ppay == 0.0m)
                    {
                        /*Logger.Log(UconomyEssentials.Instance.Translate(
                            "unable_to_pay_group_msg", Player.CharacterName, "admin"));*/
                        ppass = true;
                    }
                }
                else
                {
                    // They aren't admin so we'll just go through like groups like normal.

                    var plgroups = R.Permissions.GetGroups(Player, true);
                    foreach (var s in plgroups)
                    {
                        //Logger.Log(s.Id);
                        UconomyEssentials.Instance.premiumgroups.TryGetValue(s.Id, out var pay2);
                        //Logger.Log(pay2.ToString());
                        if (pay2 <= ppay) continue;

                        ppay = pay2;
                        paypremiumgroup = s.Id;
                    }

                    if (ppay == 0.0m)
                    {
                        // We assume they are default group.
                        UconomyEssentials.Instance.premiumgroups.TryGetValue("default", out ppay);
                        if (ppay == 0.0m)
                        {
                            // There was an error.  End it.
                            /*Logger.Log(
                                UconomyEssentials.Instance.Translate("unable_to_pay_group_msg", Player.CharacterName,
                                    ""));*/
                            ppass = true;
                        }
                    }
                }
            }
            if (!pass)
            {
                var bal = Uconomy.Instance.Database.IncreaseBalance(Player.CSteamID.ToString(), pay);
                UconomyEssentials.HandleEvent(Player, pay, "paid");
                UnturnedChat.Say(Player.CSteamID,
                    UconomyEssentials.Instance.Translate("pay_time_msg", pay,
                        Uconomy.Instance.Configuration.Instance.MoneyName, paygroup), "https://i.imgur.com/7DbFgKZ.png");
                if (bal >= 0.0m)
                    UnturnedChat.Say(Player.CSteamID,
                        UconomyEssentials.Instance.Translate("new_balance_msg", bal,
                            Uconomy.Instance.Configuration.Instance.MoneyName), "https://i.imgur.com/hOhFr7X.png");
            }
            if (!ppass)
            {
                var pbal = Uconomy.Instance.Database.IncreasePremiumBalance(Player.CSteamID.ToString(), ppay);
                UconomyEssentials.HandleEvent(Player, ppay, "ppaid");
                UnturnedChat.Say(Player.CSteamID,
                    UconomyEssentials.Instance.Translate("pay_time_msg", ppay,
                        Uconomy.Instance.Configuration.Instance.PremiumMoneyName, paypremiumgroup), "https://i.imgur.com/7DbFgKZ.png");
                if (pbal >= 0.0m)
                    UnturnedChat.Say(Player.CSteamID,
                        UconomyEssentials.Instance.Translate("new_balance_msg", pbal,
                            Uconomy.Instance.Configuration.Instance.PremiumMoneyName), "https://i.imgur.com/hOhFr7X.png");
            }
        }
    }
}