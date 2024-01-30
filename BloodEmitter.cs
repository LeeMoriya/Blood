using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;
using System.Reflection;

public class BloodEmitter : UpdatableAndDeletable
{
    public BodyChunk chunk;
    public Spear spear;
    public int bloodAmount;
    public float bleedTime = 1f;
    public float initialBleedTime;
    public Vector2 emitPos;
    public Color creatureColor;
    public string splatterColor;
    public float velocity;
    public float maxVelocity;
    public float currentTime;
    public int counter;

    public BloodEmitter(Spear spear, BodyChunk chunk, float velocity, float bleedTime)
    {
        try
        {
            //Downpour support, washes away blood splatters if enabled
            this.currentTime = Time.time;
            this.spear = spear;
            this.chunk = chunk;
            this.bleedTime = bleedTime;
            this.initialBleedTime = bleedTime;
            this.maxVelocity = velocity;
            //Default blood color
            creatureColor = new Color(0.5f, 0f, 0f);
            splatterColor = "Slugcat";
            //Individual blood colors
            if (this.chunk.owner is Creature)
            {
                //Debug.Log("Emitter: " + (this.chunk.owner as Creature).Template.type.value);
                //Get creature blood color from dictionary
                if (BloodMod.creatureColors.ContainsKey((this.chunk.owner as Creature).Template.type.value))
                {
                    creatureColor = BloodMod.creatureColors[(this.chunk.owner as Creature).Template.type.value];
                    splatterColor = (this.chunk.owner as Creature).Template.type.value;
                }
            }
        }
        catch (Exception e)
        {
            this.Destroy();
            Debug.LogException(e);
        }

    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        counter++;
        velocity = Mathf.Lerp(maxVelocity * UnityEngine.Random.Range(0.5f, 1f), -1f, Mathf.Sin(counter / 5f));

        if (emitPos.y > room.RoomRect.top + 100f)
            Destroy();

        if (chunk == null)
            Destroy();

        if ((chunk.owner is Creature) && (chunk.owner as Creature).dead)
            bleedTime -= 0.05f;
        else
            bleedTime -= 0.025f;

        if (bleedTime <= 0f)
            Destroy();

        else if ((chunk.owner is Creature) && !(chunk.owner as Creature).inShortcut)
        {
            if (spear != null && spear.stuckInAppendage != null)
                emitPos = spear.stuckInAppendage.appendage.OnAppendagePosition(spear.stuckInAppendage);
            else
                emitPos = chunk.pos;

            if (velocity >= UnityEngine.Random.Range(0.65f, 1.1f))
            {
                if (spear != null)
                    room.AddObject(new BloodParticle(emitPos, spear.rotation, creatureColor, splatterColor, this, velocity));
                else
                    room.AddObject(new BloodParticle(emitPos, Custom.RNV(), creatureColor, splatterColor, this, velocity));
            }
        }
    }
}

public class BloodParticle : CosmeticSprite
{
    public Color color;
    public float bloodAmount;
    public float bleedTime;
    public float initialBleedTime;
    public BloodEmitter emitter;
    public Vector2 angle;
    public Vector2 lastLastPos;
    public Vector2 lastLastLastPos;
    public bool collision = false;
    public string splatterColor;

    public BloodParticle(Vector2 pos, Vector2 angle, Color color, string splatterColor, BloodEmitter emitter, float vel)
    {
        this.splatterColor = splatterColor;
        lastPos = pos;
        lastLastPos = pos;
        lastLastLastPos = pos;
        this.pos = pos;
        this.color = color;
        this.emitter = emitter;
        if (this.emitter != null)
        {
            bleedTime = emitter.bleedTime;
            initialBleedTime = emitter.bleedTime;
            if (emitter.chunk == null)
            {
                this.vel = Custom.RotateAroundVector(angle, new Vector2(UnityEngine.Random.Range(-1.7f, 1.7f), vel), Custom.VecToDeg(emitter.spear.stuckInAppendage.appendage.OnAppendageDirection(emitter.spear.stuckInAppendage)) + 230f);
            }
            else
            {
                this.vel = Custom.RotateAroundVector(angle, new Vector2(UnityEngine.Random.Range(-1.7f, 1.7f), vel), Custom.VecToDeg(emitter.chunk.Rotation) + 230f);
            }
        }
        else
        {
            this.vel = angle;
            bleedTime = 1f;
            initialBleedTime = 1f;
        }
    }

    public override void Update(bool eu)
    {
        bleedTime -= 0.025f;
        if (!collision)
        {
            lastPos = pos;
            lastLastPos = lastPos;
            lastLastLastPos = lastLastPos;
            vel.y -= room.gravity;
            //Collision
            if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
            {
                Destroy();
            }
            if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid)
            {
                //Hits floor
                if (room.GetTile(pos + new Vector2(0f, 20f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles above the droplet is air, move the droplet up one tile
                    pos.y = room.MiddleOfTile(pos).y + 10f;
                }
                //Hits ceiling
                else if (room.GetTile(pos + new Vector2(0f, -20f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles below the droplet is air, move the droplet down one tile
                    pos.y = room.MiddleOfTile(pos).y - 10f;
                }
                //Hits left wall
                else if (room.GetTile(pos + new Vector2(20f, 0f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles to the right is air, move the droplet right one tile
                    pos.x = room.MiddleOfTile(pos).x + 10f;
                }
                //Hits right wall
                else if (room.GetTile(pos + new Vector2(-20f, 0f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles to the left is air, move the droplet left one tile
                    pos.x = room.MiddleOfTile(pos).x - 10f;
                }

                if (room.GetTile(pos + new Vector2(0f, 20f)).Terrain != Room.Tile.TerrainType.ShortcutEntrance || room.GetTile(pos + new Vector2(0f, 20f)).Terrain != Room.Tile.TerrainType.Solid)
                {
                    if (emitter != null)
                    {
                        if (UnityEngine.Random.value < Mathf.Lerp(0f, 0.8f, Mathf.Lerp(0f, 100f, (float)BloodMod.Options.splatterRate.Value)))
                        {
                            room.AddObject(new BloodSplatter(pos, splatterColor + "Tex", UnityEngine.Random.Range(10f, 50f)));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            room.AddObject(new BloodSplatter(pos, splatterColor + "Tex", UnityEngine.Random.Range(20f, 30f)));
                        }
                    }
                    base.slatedForDeletetion = true;
                }
                //Droplet has collided, so enable bool which slates it for deletion
                collision = true;
            }
        }
        else
        {
            base.slatedForDeletetion = true;
        }
        base.Update(eu);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
        {
            new TriangleMesh.Triangle(0, 1, 2)
        };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[0].color = Color.Lerp(color, rCam.currentPalette.blackColor, 0.3f);
        sLeaser.sprites[1] = new FSprite("Futile_White", true);
        sLeaser.sprites[1].color = Color.Lerp(color, rCam.currentPalette.blackColor, 0.3f);
        sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Spores"];
        sLeaser.sprites[1].scale = 5f;
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
        Vector2 vector2 = Vector2.Lerp(lastLastLastPos, lastLastPos, timeStacker);
        if (Custom.DistLess(vector, vector2, 9f))
        {
            vector2 = vector + Custom.DirVec(vector, vector2) * 6f;
        }
        vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, 1f));
        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + a * 1f - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - a * 1f - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
        sLeaser.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[1].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[1].rotation = UnityEngine.Random.value;
        if (emitter != null)
        {
            if (bleedTime > emitter.initialBleedTime * 0.985f)
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.8f, 0.65f + (BloodMod.goreMultiplier * 0.35f), Mathf.InverseLerp(0f, 30f, vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(3f + (BloodMod.goreMultiplier * 2), 2f, Mathf.InverseLerp(0f, 30f, vel.magnitude));
            }
            else
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.55f, 0.25f + (BloodMod.goreMultiplier * 0.2f), Mathf.InverseLerp(0f, 30f, vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(2f + (BloodMod.goreMultiplier * 1.3f), 18f, Mathf.InverseLerp(0f, 30f, vel.magnitude));
            }
        }
        else
        {
            if (bleedTime > initialBleedTime * 0.985f)
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.7f, 0.5f + (BloodMod.goreMultiplier * 0.3f), Mathf.InverseLerp(0f, 30f, vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(1f + (BloodMod.goreMultiplier * 2), 2f, Mathf.InverseLerp(0f, 30f, vel.magnitude));
            }
            else
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.5f, 0.2f + (BloodMod.goreMultiplier * 0.13f), Mathf.InverseLerp(0f, 30f, vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(1f + BloodMod.goreMultiplier, 8f, Mathf.InverseLerp(0f, 30f, vel.magnitude));
            }
        }
        if (base.slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (palette.blackColor != null)
        {
            sLeaser.sprites[0].color = Color.Lerp(color, palette.blackColor, palette.darkness * 0.8f);
            sLeaser.sprites[1].color = Color.Lerp(color, palette.blackColor, palette.darkness * 0.8f);
        }
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}
