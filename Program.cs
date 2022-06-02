using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

namespace Draven
{

    class Program
    {

        private static void OnLoadingComplete()
        {
            //Game.Print("<font color='#b756c5' size='25'>" + ObjectManager.Player.CharacterName + "</font>");
            if (ObjectManager.Player == null)
                return;
            try
            {
                switch (GameObjects.Player.CharacterName)
                {

                    case "Draven":
                        Draven.init();
                        break;
                    default:
                        Game.Print("<font color='#b756c5' size='25'>Does Not Support :" + ObjectManager.Player.CharacterName + "</font>");
                        Console.WriteLine("Not Supported " + ObjectManager.Player.CharacterName);
                        break;
                }
            }
            catch (Exception ex)
            {
                Game.Print("Error in loading");
            }
        }
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnLoadingComplete;
        }
    }

    /**
     * 
     * 
     * 
     * */
    class Draven
    {
        public static readonly float[] WBoost = { 0f, 1.4f, 1.45f, 1.5f, 1.55f, 1.6f, 1.6f };
        public static Spell Q = new Spell(SpellSlot.Q);
        public static Spell W = new Spell(SpellSlot.W);
        public static Spell E = new Spell(SpellSlot.E, 1100) { //AddHitBox = true
        };
        public static List<Mark> futou = new List<Mark>();
        public static void init()
        {
            E.SetSkillshot(0, 150, float.MaxValue, false, SpellType.Line,HitChance.None);
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;

            Game.OnUpdate += OnGameTick;
            Drawing.OnDraw += OnDraw;
            Game.Print("Draven initing-");

            AIHeroClient.OnDoCast += OnProcessSpellCast;
            AIHeroClient.OnProcessSpellCast += OnProcessSpellCast;



        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
           if (!sender.IsMe && args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E || args.Slot == SpellSlot.R)Game.Print(">>>>>"+sender.Name + ">>>>>" + args.SData.Name);

            if ( GameObjects.EnemyHeroes.Any(x=>x.Name == sender.Name) && args.SData.Name.Contains("BasicAttack") && args.Target.IsMe && sender.IsMelee) {
                E.Cast(sender.Position);
                return;
            }
           if (sender.IsSuppressed){
                E.Cast(sender.Position);
                return;

           }
           if (args.SData.Name == "Headbutt" && args.Target.IsMe) {
                E.Cast(sender.Position);
                return;
            }
           if (args.SData.Name == "JaxLeapStrike" && args.Target.IsMe) {
                E.Cast(sender.Position);
                return;
            }   
           if (args.SData.Name == "MonkeyKingNimbus" && args.Target.IsMe){
                E.Cast(sender.Position);
                return;
            }

        }

        private static void OnDraw(EventArgs args)
        {
            foreach (var x in futou)
            {
                EnsoulSharp.SDK.Rendering.CircleRender.Draw(x.Position, x.Object.BoundingRadius, Color.Red);
            }
        }

        private static void OnGameTick(EventArgs args)
        {

            cathchAXE();
            castQ();

        }

        static void castQ() {

            if (GameObjects.Player.Position.CountEnemyHeroesInRange(GameObjects.Player.GetCurrentAutoAttackRange()+10) > 0
            && GameObjects.Player.GetBuffCount("DravenSpinningAttack") < 2
            && Orbwalker.ActiveMode ==OrbwalkerMode.Combo) {
                if (Q.IsReady()) {
                    Q.Cast();
                }
            }

        }

        static void cathchAXE() {

            Orbwalker.SetOrbwalkerPosition(Game.CursorPos);

            foreach (var x in futou)
            {
                if (x.Position.DistanceToCursor() <= (Game.CursorPos.DistanceToPlayer()) || x.Position.DistanceToPlayer() <= 670)
                {   
                    if (x == null || !x.Object.IsValid || x.Position.IsUnderEnemyTurret() || x.Position.IsBuilding() || x.Position.IsWall() || Game.Time > x.EndTime ||
                        x.Position.DistanceToPlayer() <= 110 || futou.Count < 1 || GameObjects.Player.GetBuffCount("DravenSpinningAttack") == 2
                        )
                    {
                        Orbwalker.SetOrbwalkerPosition(Game.CursorPos);
                        continue;
 

                    }
                    if ((GameObjects.Player.MoveSpeed * (x.EndTime - Game.Time) >= (x.Object.DistanceToPlayer()+50)))
                    {
                        //Game.Print("normal catch");
                        Orbwalker.SetOrbwalkerPosition(x.Position);
                        break;
                    }else
                    {
                        Game.Print("calc catch"+W.Level);
                        var boosSpeed = GameObjects.Player.MoveSpeed * WBoost[W.Level];
                        if (boosSpeed * (x.EndTime - Game.Time) >= (x.Object.DistanceToPlayer()))
                        {
                            W.Cast();
                            Orbwalker.SetOrbwalkerPosition(x.Position);
                            break;
                        }

                    }


                    Orbwalker.SetOrbwalkerPosition(Game.CursorPos);
                    continue;
                }
            }

        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Draven_") && sender.Name.Contains("_Q_reticle_self"))
            {

                futou.RemoveAll(x => x.NetworkId == sender.NetworkId);
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Draven_") && sender.Name.Contains("_Q_reticle_self"))
            {
                futou.Add(new Mark(sender,sender.NetworkId,sender.Position,Variables.TickCount + 1300));
            }
        }

        public class Mark
        {
            public Mark(GameObject obj, int newtworkId, Vector3 pos, int endTime)
            {
                this.Object = obj;
                this.NetworkId = newtworkId;
                this.Position = pos;
                this.EndTime = endTime;
            }
            public GameObject Object { get; }
            public int NetworkId { get; }
            public Vector3 Position { get; }
            public int EndTime { get; }
        }











    }
}
