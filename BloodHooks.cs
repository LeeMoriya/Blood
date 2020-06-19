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
        On.Spear.LodgeInCreature += Spear_LodgeInCreature;
        On.Lizard.Bite += Lizard_Bite;
        On.BigNeedleWorm.Swish += BigNeedleWorm_Swish;
        On.Vulture.Carry += Vulture_Carry;
        On.Room.Update += Room_Update;
        On.Spear.TryImpaleSmallCreature += Spear_TryImpaleSmallCreature;
        On.Fly.BitByPlayer += Fly_BitByPlayer;
    }

    private static void Fly_BitByPlayer(On.Fly.orig_BitByPlayer orig, Fly self, Creature.Grasp grasp, bool eu)
    {
        orig.Invoke(self, grasp, eu);
        for (int i = 0; i < 2; i++)
        {
            self.room.AddObject(new BloodParticle(self.mainBodyChunk.pos, new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 5f)), BloodMod.creatureColors["Fly"], "FlyTex", null, 1.3f));
        }
    }

    private static void Spear_TryImpaleSmallCreature(On.Spear.orig_TryImpaleSmallCreature orig, Spear self, Creature smallCrit)
    {
        orig.Invoke(self, smallCrit);
    }

    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        //Track creatures which have been grabbed by vultures, remove them from the list if no longer grabbed
        orig.Invoke(self);
        if (BloodMod.chunkTracker != null && BloodMod.chunkTracker.Count > 0)
        {
            for (int i = 0; i < BloodMod.chunkTracker.Count; i++)
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
    }

    private static void Vulture_Carry(On.Vulture.orig_Carry orig, Vulture self)
    {
        //Add creature to a list once grabbed to prevent multiple emitters being spawned
        orig.Invoke(self);
        if (self.grasps[0].grabbedChunk.owner is Creature)
        {
            if (!BloodMod.chunkTracker.Contains(self.grasps[0].grabbedChunk))
            {
                self.room.AddObject(new BloodEmitter(null, self.grasps[0].grabbedChunk, 4f, 0.8f));
                BloodMod.chunkTracker.Add(self.grasps[0].grabbedChunk);
            }
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

    private static void Spear_LodgeInCreature(On.Spear.orig_LodgeInCreature orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        //Create an emitter when a create is impaled by a spear
        orig.Invoke(self, result, eu);
        if (self.stuckInChunk != null)
        {
            self.room.AddObject(new BloodEmitter(self, self.stuckInChunk, UnityEngine.Random.Range(5f, 8f), UnityEngine.Random.Range(1f, 3f)));
            Debug.Log("Blood: Creating new blood emitter");
        }
    }
}

