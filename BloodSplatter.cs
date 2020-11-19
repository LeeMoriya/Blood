using System;
using System.IO;
using RWCustom;
using UnityEngine;
using Partiality;
using Partiality.Modloader;
using System.Reflection;

public class BloodSplatter : UpdatableAndDeletable, IDrawable
{
    public Vector2 pos;
    public Vector2[] quad;
    public Vector2[] verts;
    public string color;
    public float scale;
    public float[,] vertices;
    public bool meshDirty;
    public bool elementDirty;
    public int gridDiv;
    public bool wash;
    public float fade;
    public bool washable;
    public bool once;
    public IntVector2? skyPosition;
    public float alphaFade;
    public float rainIntensity;
    public float dripTime;
    public bool drip;
    public BloodSplatter(Vector2 pos, string color, float scale)
    {
        this.washable = false;
        this.once = false;
        this.fade = 0f;
        this.color = color;
        this.pos = pos;
        this.scale = scale;
        this.gridDiv = 1;
        this.dripTime = UnityEngine.Random.Range(1f,2f);
        this.quad = new Vector2[4];
        this.quad[0] = this.pos + new Vector2(-this.scale, this.scale);
        this.quad[1] = this.pos + new Vector2(this.scale, this.scale);
        this.quad[3] = this.pos + new Vector2(-this.scale, -this.scale);
        this.quad[2] = this.pos + new Vector2(this.scale, -this.scale);
        this.gridDiv = this.GetIdealGridDiv();
        this.vertices = new float[4, 2];
        for (int i = 0; i < this.vertices.GetLength(0); i++)
        {
            this.vertices[i, 0] = 0.5f;
        }
        this.meshDirty = true;
    }

    public void UpdateMesh()
    {
        this.meshDirty = true;
    }

    public override void Update(bool eu)
    {
        if (!BloodMod.wash)
        {
            if (BloodMod.compat)
            {
                foreach (PartialityMod mod in PartialityManager.Instance.modManager.loadedMods)
                {
                    if (mod.ModID == "Downpour")
                    {
                        FieldInfo r = mod.GetType().Assembly.GetType("RainFall").GetField("rainIntensity");
                        if(r!= null)
                        {
                            this.rainIntensity = (float)r.GetValue(null);
                        }
                        FieldInfo f = mod.GetType().GetField("snow");
                        if (f != null && !(bool)f.GetValue(mod))
                        {
                            BloodMod.wash = true;
                            Debug.Log("Blood washing enabled");
                        }
                    }
                }
            }
        }
        if (!this.once && BloodMod.wash && BloodMod.compat)
        {
            foreach (PartialityMod mod in PartialityManager.Instance.modManager.loadedMods)
            {
                if (mod.ModID == "Downpour")
                {
                    FieldInfo r = mod.GetType().Assembly.GetType("RainFall").GetField("rainIntensity");
                    if (r != null)
                    {
                        this.rainIntensity = (float)r.GetValue(null);
                    }
                }
            }
            if (RayTraceSky(new Vector2(0f, 5f)))
            {
                this.washable = true;
            }
            if (RayTraceSky(new Vector2(-1f, 5f)))
            {
                this.washable = true;
            }
            if (RayTraceSky(new Vector2(1f, 5f)))
            {
                this.washable = true;
            }
            this.once = true;
        }
        base.Update(eu);
    }

    public int GetIdealGridDiv()
    {
        float num = 0f;
        for (int i = 0; i < 3; i++)
        {
            if (Vector2.Distance(this.quad[i], this.quad[i + 1]) > num)
            {
                num = Vector2.Distance(this.quad[i], this.quad[i + 1]);
            }
        }
        if (Vector2.Distance(this.quad[0], this.quad[3]) > num)
        {
            num = Vector2.Distance(this.quad[0], this.quad[3]);
        }
        return Mathf.Clamp(Mathf.RoundToInt(num / 150f), 1, 20);
    }
    public bool RayTraceSky(Vector2 testDir)
    {
        Vector2 corner = Custom.RectCollision(this.quad[0], this.quad[0] + testDir * 100000f, this.room.RoomRect).GetCorner(FloatRect.CornerLabel.D);
        if (SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, this.quad[0] + testDir, corner) != null)
        {
            return false;
        }
        if (corner.y >= this.room.PixelHeight - 5f)
        {
            return true;
        }
        return false;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh(this.color, this.gridDiv);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Decal"];
        this.verts = new Vector2[(sLeaser.sprites[0] as TriangleMesh).vertices.Length];
        this.AddToContainer(sLeaser, rCam, null);
        this.meshDirty = true;
    }

    public void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites[0].RemoveFromContainer();
        this.InitiateSprites(sLeaser, rCam);
        float[,] vertices = this.vertices;
        alphaFade = Mathf.Lerp(1f,0.1f, rCam.currentPalette.darkness * 0.9f);
        for (int i = 0; i <= this.gridDiv; i++)
        {
            for (int j = 0; j <= this.gridDiv; j++)
            {
                Vector2 a = Vector2.Lerp(this.quad[0], this.quad[1], (float)j / (float)this.gridDiv);
                Vector2 b = Vector2.Lerp(this.quad[1], this.quad[2], (float)i / (float)this.gridDiv);
                Vector2 b2 = Vector2.Lerp(this.quad[3], this.quad[2], (float)j / (float)this.gridDiv);
                Vector2 a2 = Vector2.Lerp(this.quad[0], this.quad[3], (float)i / (float)this.gridDiv);
                float num = Mathf.Lerp(Mathf.Lerp(vertices[3, 1], vertices[2, 1], (float)i / (float)this.gridDiv), Mathf.Lerp(vertices[0, 1], vertices[1, 1], (float)i / (float)this.gridDiv), (float)j / (float)this.gridDiv);
                float num2 = Mathf.Lerp(Mathf.Lerp(vertices[3, 0], vertices[2, 0], (float)i / (float)this.gridDiv), Mathf.Lerp(vertices[0, 0], vertices[1, 0], (float)i / (float)this.gridDiv), (float)j / (float)this.gridDiv);
                num = Mathf.Pow(num, 1f + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value) * 0.1f);
                num2 = Mathf.Pow(num2, 1f + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value) * 0.1f);
                num = Mathf.Lerp(num, UnityEngine.Random.value, 0.1f * Mathf.Pow(1f - 2f * Mathf.Abs(num - 0.1f), 2.5f));
                num2 = Mathf.Lerp(num2, UnityEngine.Random.value, 0.1f * Mathf.Pow(1f - 2f * Mathf.Abs(num - 0.3f), 2.5f));
                this.verts[j * (this.gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
                (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (this.gridDiv + 1) + i] = new Color(0f, 0.25f, num, alphaFade);
            }
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (this.washable)
        {
            if (this.rainIntensity > 0)
            {
                float washRate = Mathf.Lerp(0, 0.0065f, this.rainIntensity);
                this.meshDirty = false;
                for (int i = 0; i <= this.gridDiv; i++)
                {
                    for (int j = 0; j <= this.gridDiv; j++)
                    {
                        (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (this.gridDiv + 1) + i].a = (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (this.gridDiv + 1) + i].a - washRate * timeStacker;
                        if ((sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (this.gridDiv + 1) + i].a <= 0f)
                        {
                            base.slatedForDeletetion = true;
                        }
                    }
                }
            }
        }
        if (this.meshDirty)
        {
            this.UpdateVerts(sLeaser, rCam);
            this.meshDirty = false;
        }
        if (this.elementDirty)
        {
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("PH");
            this.elementDirty = false;
        }
        //0 - Bottom Left
        //1 - Top Left
        //2 - Top Right
        //3 - Bottom Right
        for (int i = 0; i < this.verts.Length; i++)
        {
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, this.verts[i] - camPos);
        }
        if (base.slatedForDeletetion || this.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {

    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.sprites[0].RemoveFromContainer();
        rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
    }
}
