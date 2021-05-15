using System;
using System.Linq;
using OKTW;
using OKTW.Prediction;
using LPRebornSDK.LeagueSharp.SDKConvert;
using LPRebornSDK.LeagueSharp;


namespace ARAMShooter
{
    internal class Program
    {
        public static Spell Throw;
        public static Menu Menu;
        public static HitChance MinHitChance;

        public static void Main(string[] args)
        {
            AIBaseClient.OnStartup += AIBaseClient_OnStartup;
        }

        private static void AIBaseClient_OnStartup()
        {
            var spell = ObjectManager.Player.GetSpellSlot("summonersnowball");

            if (spell == SpellSlot.Unknown)
            {
                return;
            }
            

            Menu = new Menu("ARAMShooter", "ARAMShooter", true);
            Menu.AddItem(new MenuItem("DecreaseRange", "Decrease Range by").SetValue(new Slider(10)));
            Menu.AddItem(
                new MenuItem("HitChance", "MinHitChance" ).SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));
            Menu.AddItem(new MenuItem("Auto", "AutoDash").SetValue(true));
            Menu.ItemAt("HitChance").ValueChanged += Program_ValueChanged;
            Menu.ItemAt("DecreaseRange").ValueChanged += Program_ValueChanged1;
            Menu.AddToMainMenu();

            Throw = new Spell(spell, 2500f);
            Throw.SetSkillshot(.33f, 50f, 1600, true, SkillshotType.SkillshotLine);
            MinHitChance = GetHitChance();

            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate()
        {
            if (!Throw.IsReady())
            {
                return;
            }

            if (Throw.Name.Equals("snowballfollowupcast"))
            {
                if (!Menu.ItemAt("Auto").IsActive())
                {
                    return;
                }

                Throw.Cast();
                return;
            }

            foreach (var champ in
                ObjectManager.Get<AIHeroClient>().Where(h => h.IsValidTarget(Throw.Range)))
            {
                if (Throw.GetPrediction(champ).Hitchance >= MinHitChance)
                    Throw.Cast(champ);
                else
                    continue;
            }
        }

        private static void Program_ValueChanged1(object sender, OnValueChangeEventArgs e)
        {
            Throw.Range = 2500f - e.GetNewValue<Slider>().Value;
        }

        private static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            MinHitChance = GetHitChance();
        }

        private static HitChance GetHitChance()
        {
            var hc = Menu.ItemAt("HitChance").GetValue<StringList>();
            switch (hc.SList[hc.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }
    }
}