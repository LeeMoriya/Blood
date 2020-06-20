using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;
using System.Reflection;
using Partiality;
using Partiality.Modloader;

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

    public BloodEmitter(Spear spear, BodyChunk chunk, float velocity, float bleedTime)
    {
        //Downpour support, washes away blood splatters if enabled
        this.currentTime = Time.time;
        this.spear = spear;
        this.chunk = chunk;
        this.bleedTime = bleedTime;
        this.initialBleedTime = bleedTime;
        this.maxVelocity = velocity + (BloodMod.goreMultiplier * 3);
        //Default blood color
        this.creatureColor = new Color(0.5f, 0f, 0f);
        this.splatterColor = "red";
        //Individual blood colors
        if (this.chunk.owner is Creature)
        {
            //Get creature blood color from dictionary
            if (BloodMod.creatureColors.ContainsKey((this.chunk.owner as Creature).Template.type.ToString()))
            {
                this.creatureColor = BloodMod.creatureColors[(this.chunk.owner as Creature).Template.type.ToString()];
                this.splatterColor = (this.chunk.owner as Creature).Template.type.ToString() + "Tex";
            }
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        this.velocity = Mathf.Lerp(maxVelocity * UnityEngine.Random.Range(0.5f, 1f), -1f, Mathf.PingPong(Time.time - this.currentTime, 1));
        if (this.emitPos.y > this.room.RoomRect.top + 100f)
        {
            Destroy();
        }
        if (this.chunk == null)
        {
            this.Destroy();
        }
        if ((this.chunk.owner as Creature).dead)
        {
            //Speed up bleed time if the creature is dead
            this.bleedTime = this.bleedTime - 0.8f * Time.deltaTime;
        }
        else
        {
            this.bleedTime = this.bleedTime - 0.5f * Time.deltaTime;
        }
        if (this.bleedTime <= 0f)
        {
            this.Destroy();
        }
        else if (!(this.chunk.owner as Creature).inShortcut)
        {
            if (this.spear != null && this.spear.stuckInAppendage != null)
            {
                this.emitPos = this.spear.stuckInAppendage.appendage.OnAppendagePosition(this.spear.stuckInAppendage);
            }
            else
            {
                this.emitPos = this.chunk.pos;
            }
            if (this.velocity >= UnityEngine.Random.Range(0.65f, 1.1f))
            {
                if (this.spear != null)
                {
                    this.room.AddObject(new BloodParticle(this.emitPos, this.spear.lastRotation, this.creatureColor, this.splatterColor, this, velocity));
                }
                else
                {
                    this.room.AddObject(new BloodParticle(this.emitPos, Custom.RNV(), this.creatureColor, this.splatterColor, this, velocity));
                }
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
    public bool downpour;

    public BloodParticle(Vector2 pos, Vector2 angle, Color color, string splatterColor, BloodEmitter emitter, float vel)
    {
        this.splatterColor = splatterColor;
        this.lastPos = pos;
        this.lastLastPos = pos;
        this.lastLastLastPos = pos;
        this.pos = pos;
        this.color = color;
        this.emitter = emitter;
        if (this.emitter != null)
        {
            this.bleedTime = this.emitter.bleedTime;
            this.initialBleedTime = this.emitter.bleedTime;
            if (this.emitter.chunk == null)
            {
                this.vel = Custom.RotateAroundVector(angle, new Vector2(UnityEngine.Random.Range(-1.7f, 1.7f), vel), Custom.VecToDeg(this.emitter.spear.stuckInAppendage.appendage.OnAppendageDirection(this.emitter.spear.stuckInAppendage)) + 230f);
            }
            else
            {
                this.vel = Custom.RotateAroundVector(angle, new Vector2(UnityEngine.Random.Range(-1.7f, 1.7f), vel), Custom.VecToDeg(this.emitter.chunk.Rotation) + 230f);
            }
        }
        else
        {
            this.vel = angle;
            this.bleedTime = 1f;
            this.initialBleedTime = 1f;
        }
    }

    public override void Update(bool eu)
    {
        this.bleedTime = this.bleedTime - 0.2f * Time.deltaTime;
        if (!collision)
        {
            this.lastPos = this.pos;
            this.lastLastPos = this.lastPos;
            this.lastLastLastPos = this.lastLastPos;
            this.vel.y -= this.room.gravity;
            //Collision
            if (this.room.GetTile(this.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
            {
                this.Destroy();
            }
            if (this.room.GetTile(this.pos).Terrain == Room.Tile.TerrainType.Solid)
            {
                //Hits floor
                if (this.room.GetTile(this.pos + new Vector2(0f, 20f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles above the droplet is air, move the droplet up one tile
                    this.pos.y = this.room.MiddleOfTile(this.pos).y + 10f;
                }
                //Hits ceiling
                else if (this.room.GetTile(this.pos + new Vector2(0f, -20f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles below the droplet is air, move the droplet down one tile
                    this.pos.y = this.room.MiddleOfTile(this.pos).y - 10f;
                }
                //Hits left wall
                else if (this.room.GetTile(this.pos + new Vector2(20f, 0f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles to the right is air, move the droplet right one tile
                    this.pos.x = this.room.MiddleOfTile(this.pos).x + 10f;
                }
                //Hits right wall
                else if (this.room.GetTile(this.pos + new Vector2(-20f, 0f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    //If two tiles to the left is air, move the droplet left one tile
                    this.pos.x = this.room.MiddleOfTile(this.pos).x - 10f;
                }

                if (this.room.GetTile(this.pos + new Vector2(0f, 20f)).Terrain != Room.Tile.TerrainType.ShortcutEntrance || this.room.GetTile(this.pos + new Vector2(0f, 20f)).Terrain != Room.Tile.TerrainType.Solid)
                {
                    if (this.emitter != null)
                    {
                        if (UnityEngine.Random.value > 0.5f - (BloodMod.goreMultiplier * 0.3f))
                        {
                            this.room.AddObject(new BloodSplatter(this.pos, this.splatterColor, UnityEngine.Random.Range(10f, 50f + BloodMod.goreMultiplier * 25)));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            this.room.AddObject(new BloodSplatter(this.pos, this.splatterColor, UnityEngine.Random.Range(20f, 30f)));
                        }
                    }
                    base.slatedForDeletetion = true;
                }
                //Droplet has collided, so enable bool which slates it for deletion
                this.collision = true;
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
        sLeaser.sprites[0].color = Color.Lerp(this.color, this.room.game.cameras[0].currentPalette.blackColor, 0.3f);
        sLeaser.sprites[1] = new FSprite("Futile_White", true);
        sLeaser.sprites[1].color = Color.Lerp(this.color, this.room.game.cameras[0].currentPalette.blackColor, 0.3f);
        sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Spores"];
        sLeaser.sprites[1].scale = 5f;
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
        Vector2 vector2 = Vector2.Lerp(this.lastLastLastPos, this.lastLastPos, timeStacker);
        if (Custom.DistLess(vector, vector2, 9f))
        {
            vector2 = vector + Custom.DirVec(vector, vector2) * 6f;
        }
        vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, 1f));
        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + a * 1f - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - a * 1f - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
        sLeaser.sprites[1].x = Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[1].y = Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[1].rotation = UnityEngine.Random.value;
        if (this.emitter != null)
        {
            if (this.bleedTime > this.emitter.initialBleedTime * 0.985f)
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.8f, 0.65f + (BloodMod.goreMultiplier * 0.35f), Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(3f + (BloodMod.goreMultiplier * 2), 2f, Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
            }
            else
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.55f, 0.25f + (BloodMod.goreMultiplier * 0.2f), Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(2f + (BloodMod.goreMultiplier * 1.3f), 18f, Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
            }
        }
        else
        {
            if (this.bleedTime > this.initialBleedTime * 0.985f)
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.7f, 0.5f + (BloodMod.goreMultiplier * 0.3f), Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(1f + (BloodMod.goreMultiplier * 2), 2f, Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
            }
            else
            {
                sLeaser.sprites[1].alpha = Mathf.Lerp(0.5f, 0.2f + (BloodMod.goreMultiplier * 0.13f), Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
                sLeaser.sprites[1].scale = Mathf.Lerp(1f + BloodMod.goreMultiplier, 8f, Mathf.InverseLerp(0f, 30f, this.vel.magnitude));
            }
        }
        if (base.slatedForDeletetion || this.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}
