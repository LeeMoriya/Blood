using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
public class BloodHooks
{
    public static void Hook()
    {
        On.Spear.LodgeInCreature_CollisionResult_bool += Spear_LodgeInCreature_CollisionResult_bool;
        On.Lizard.Bite += Lizard_Bite;
        On.BigNeedleWorm.Swish += BigNeedleWorm_Swish;
        On.Vulture.Carry += Vulture_Carry;
        On.Room.Update += Room_Update;
        On.Fly.BitByPlayer += Fly_BitByPlayer;
        On.KingTusks.ThisCreatureImpaled += KingTusks_ThisCreatureImpaled;
        On.EggBugEgg.BitByPlayer += EggBugEgg_BitByPlayer;
        On.JellyFish.BitByPlayer += JellyFish_BitByPlayer;
        On.SmallNeedleWorm.BitByPlayer += SmallNeedleWorm_BitByPlayer;
        On.Centipede.BitByPlayer += Centipede_BitByPlayer;
        On.VultureGrub.BitByPlayer += VultureGrub_BitByPlayer;
        On.Hazer.BitByPlayer += Hazer_BitByPlayer;
        On.Player.EatMeatUpdate += Player_EatMeatUpdate;
        On.Rock.HitSomething += Rock_HitSomething;
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorld.Update += RainWorld_Update;
    }

    private static void Spear_LodgeInCreature_CollisionResult_bool(On.Spear.orig_LodgeInCreature_CollisionResult_bool orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        //Create an emitter when a create is impaled by a spear
        orig.Invoke(self, result, eu);
        if (self != null && self.stuckInChunk != null)
        {
            self.room.AddObject(new BloodEmitter(self, self.stuckInChunk, UnityEngine.Random.Range(5f, 8f), UnityEngine.Random.Range(1f, 3f)));
        }
    }

    private static void Player_EatMeatUpdate(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
    {
        if (self.grasps[graspIndex] != null && self.grasps[graspIndex].grabbed is Creature)
        {
            if ((self.eatMeat > 45 && self.eatMeat < 50) || (self.eatMeat > 55 && self.eatMeat < 60) || (self.eatMeat > 65 && self.eatMeat < 75))
            {
                self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(5f, 10f)), BloodMod.creatureColors[(self.grasps[graspIndex].grabbedChunk.owner as Creature).Template.type.ToString()], (self.grasps[graspIndex].grabbedChunk.owner as Creature).Template.type.ToString(), null, 2.3f));
            }
        }
        orig.Invoke(self, graspIndex);
    }

    private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        try
        {
            orig.Invoke(self);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        BloodMod.chunkTracker = new List<BodyChunk>();
        orig.Invoke(self, manager);
    }

    private static bool Rock_HitSomething(On.Rock.orig_HitSomething orig, Rock self, SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj == null)
        {
            return false;
        }
        if (result.obj is Creature)
        {
            self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(5f, 10f)), BloodMod.creatureColors[(result.obj as Creature).Template.type.ToString()], (result.obj as Creature).Template.type.ToString(), null, 2f));
        }
        orig.Invoke(self, result, eu);
        return true;
    }

    //Small Creatures
    private static void Hazer_BitByPlayer(On.Hazer.orig_BitByPlayer orig, Hazer self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["Hazer"], "Hazer", null, 1.3f));
        }
    }

    private static void VultureGrub_BitByPlayer(On.VultureGrub.orig_BitByPlayer orig, VultureGrub self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["VultureGrub"], "VultureGrub", null, 1.3f));
        }
    }

    private static void Centipede_BitByPlayer(On.Centipede.orig_BitByPlayer orig, Centipede self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["SmallCentipede"], "SmallCentipede", null, 1.3f));
        }
    }

    private static void SmallNeedleWorm_BitByPlayer(On.SmallNeedleWorm.orig_BitByPlayer orig, SmallNeedleWorm self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["SmallNeedleWorm"], "SmallNeedleWorm", null, 1.3f));
        }
    }

    private static void JellyFish_BitByPlayer(On.JellyFish.orig_BitByPlayer orig, JellyFish self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["Hazer"], "Hazer", null, 1.3f));
        }
    }

    private static void EggBugEgg_BitByPlayer(On.EggBugEgg.orig_BitByPlayer orig, EggBugEgg self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.bodyChunks[0].pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["EggBug"], "EggBug", null, 1.3f));
        }
    }
    private static void Fly_BitByPlayer(On.Fly.orig_BitByPlayer orig, Fly self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.mainBodyChunk.pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["Fly"], "Fly", null, 1.3f));
        }
    }


    //---------------------------------------------
    //Really bad ChunkTracker stuff needs replacing
    //---------------------------------------------
    private static void Vulture_Carry(On.Vulture.orig_Carry orig, Vulture self)
    {
        //Add creature to a list once grabbed to prevent multiple emitters being spawned
        orig.Invoke(self);
        if (self != null & self.grasps[0] != null && self.grasps[0].grabbedChunk != null && self.grasps[0].grabbedChunk.owner is Creature)
        {
            if (BloodMod.chunkTracker != null && !BloodMod.chunkTracker.Contains(self.grasps[0].grabbedChunk))
            {
                self.room.AddObject(new BloodEmitter(null, self.grasps[0].grabbedChunk, 4f, 0.8f));
                BloodMod.chunkTracker.Add(self.grasps[0].grabbedChunk);
            }
        }
    }

    //Impaled by KingTusks
    private static bool KingTusks_ThisCreatureImpaled(On.KingTusks.orig_ThisCreatureImpaled orig, KingTusks self, AbstractCreature crit)
    {
        for (int i = 0; i < self.tusks.Length; i++)
        {
            if (self.tusks[i].impaleChunk != null && self.tusks[i].impaleChunk.owner is Creature && (self.tusks[i].impaleChunk.owner as Creature).abstractCreature == crit)
            {
                if (!BloodMod.chunkTracker.Contains(self.tusks[i].impaleChunk))
                {
                    //self.vulture.room.AddObject(new BloodEmitter(null, crit.realizedCreature.mainBodyChunk, 8f, 4f));
                    if(crit.realizedCreature != null && crit.realizedCreature.room != null)
                    {
                        crit.realizedCreature.room.AddObject(new BloodEmitter(null, self.tusks[i].impaleChunk, UnityEngine.Random.Range(7f, 8f), 1.5f));
                    }
                    BloodMod.chunkTracker.Add(self.tusks[i].impaleChunk);
                }
                return true;
            }
        }
        return orig.Invoke(self,crit);
    }

    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        //Track creatures which have been grabbed by vultures, remove them from the list if no longer grabbed
        orig.Invoke(self);
        try
        {
            if (BloodMod.chunkTracker != null && BloodMod.chunkTracker.Count > 0)
            {
                for (int i = BloodMod.chunkTracker.Count - 1; i >= 0; i--)
                {
                    if (BloodMod.chunkTracker[i].owner is Creature)
                    {
                        if ((BloodMod.chunkTracker[i].owner as Creature).grabbedBy == null)
                        {
                            BloodMod.chunkTracker.RemoveAt(i);
                        }
                    }
                }
            }
            if (BloodMod.spikeCompat)
            {
                foreach (var item in self.updateList)
                {
                    if(item is RegionKit.Modules.Objects.Spike)
                    {
                        var spike = item as RegionKit.Modules.Objects.Spike;
                        if(spike.impaledCreature != null && spike.impaledChunk != null)
                        {
                            if (BloodMod.creatureColors.ContainsKey(spike.impaledCreature.Template.type.value))
                            {
                                if (spike.tipColor != BloodMod.creatureColors[spike.impaledCreature.Template.type.value])
                                {
                                    spike.tipColor = BloodMod.creatureColors[spike.impaledCreature.Template.type.value];
                                    spike.updateTipColor = true;
                                    Debug.Log("Changed spike tip color");
                                }
                            }
                        }
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    private static void BigNeedleWorm_Swish(On.BigNeedleWorm.orig_Swish orig, BigNeedleWorm self)
    {
        //Create emitter when impaled by NeedleWorm
        orig.Invoke(self);
        if (self.impaleChunk != null && self.impaleChunk.owner is Creature)
        {
            self.room.AddObject(new BloodEmitter(null, self.impaleChunk, UnityEngine.Random.Range(7f, 8f), 1.5f));
        }
    }

    private static void Lizard_Bite(On.Lizard.orig_Bite orig, Lizard self, BodyChunk chunk)
    {
        //Create small emitter when a Lizard bites are creature
        orig.Invoke(self, chunk);
        if (chunk != null && chunk.owner is Creature)
        {
            self.room.AddObject(new BloodEmitter(null, chunk, UnityEngine.Random.Range(1f, 5f), 0.08f));
        }
    }
}

