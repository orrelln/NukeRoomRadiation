using System;
using System.Collections.Generic;

using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Exiled.Events.EventArgs.Server;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;

namespace NukeRoomRadiation
{
    public class EventHandlers
    {
        private static Plugin plugin;
        private static List<CoroutineHandle> Coroutines = new List<CoroutineHandle> { };
        private static Dictionary<string, int> SecondsInNuke = new Dictionary<string, int>();
        public EventHandlers(Plugin P) => plugin = P;

        public static void Scan()
        {
            try
            {
                foreach (Player CurPlayer in Player.List)
                {
                    if (CurPlayer.IsAlive && CurPlayer.Role.Team != Team.OtherAlive && CurPlayer.CurrentRoom.Type == RoomType.HczNuke && 
                        CurPlayer.Position.y > -800)
                    {
                        if (SecondsInNuke[CurPlayer.UserId]++ >= plugin.Config.RadiationDelay)
                        {
                            float shields = CurPlayer.HumeShield + CurPlayer.ArtificialHealth;
                            CurPlayer.EnableEffect(EffectType.Burned, 1f);
                            CurPlayer.EnableEffect(EffectType.DamageReduction, 1f);
                            CurPlayer.ChangeEffectIntensity(EffectType.DamageReduction, 40);
                            CurPlayer.Hurt(shields >= plugin.Config.RadiationDamage ? plugin.Config.RadiationDamage : shields, DamageType.Poison);
                        }
                       
                    }
                    else
                    {
                        SecondsInNuke[CurPlayer.UserId] = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public IEnumerator<float> ScanLoop()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);
                Scan();
            }
        }

        public void OnRoundStarted()
        {
            Coroutines.Add(Timing.RunCoroutine(ScanLoop()));
            
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (CoroutineHandle CHandle in Coroutines)
            {
                Timing.KillCoroutines(CHandle);
            }
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            SecondsInNuke[ev.Player.UserId] = 0;
        }

        public void OnDetonated()
        {
            foreach (CoroutineHandle CHandle in Coroutines)
            {
                Timing.KillCoroutines(CHandle);
            }
        }
    }
}