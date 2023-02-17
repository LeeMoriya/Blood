using System;
using System.IO;
using RWCustom;
using UnityEngine;
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
        washable = false;
        once = false;
        fade = 0f;
        this.color = color;
        this.pos = pos;
        this.scale = scale;
        gridDiv = 1;
        dripTime = UnityEngine.Random.Range(1f,2f);
        quad = new Vector2[4];
        quad[0] = pos + new Vector2(-scale, scale);
        quad[1] = pos + new Vector2(scale, scale);
        quad[3] = pos + new Vector2(-scale, -scale);
        quad[2] = pos + new Vector2(scale, -scale);
        gridDiv = GetIdealGridDiv();
        vertices = new float[4, 2];
        for (int i = 0; i < vertices.GetLength(0); i++)
        {
            vertices[i, 0] = 0.5f;
        }
        meshDirty = true;
    }

    public void UpdateMesh()
    {
        meshDirty = true;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }

    public int GetIdealGridDiv()
    {
        float num = 0f;
        for (int i = 0; i < 3; i++)
        {
            if (Vector2.Distance(quad[i], quad[i + 1]) > num)
            {
                num = Vector2.Distance(quad[i], quad[i + 1]);
            }
        }
        if (Vector2.Distance(quad[0], quad[3]) > num)
        {
            num = Vector2.Distance(quad[0], quad[3]);
        }
        return Mathf.Clamp(Mathf.RoundToInt(num / 150f), 1, 20);
    }
    public bool RayTraceSky(Vector2 testDir)
    {
        Vector2 corner = Custom.RectCollision(quad[0], quad[0] + testDir * 100000f, room.RoomRect).GetCorner(FloatRect.CornerLabel.D);
        if (SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, quad[0] + testDir, corner) != null)
        {
            return false;
        }
        if (corner.y >= room.PixelHeight - 5f)
        {
            return true;
        }
        return false;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh(color, gridDiv);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Decal"];
        verts = new Vector2[(sLeaser.sprites[0] as TriangleMesh).vertices.Length];
        AddToContainer(sLeaser, rCam, null);
        meshDirty = true;
    }

    public void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites[0].RemoveFromContainer();
        InitiateSprites(sLeaser, rCam);
        float[,] vertices = this.vertices;
        alphaFade = Mathf.Lerp(1f,0.1f, rCam.currentPalette.darkness * 0.9f);
        for (int i = 0; i <= gridDiv; i++)
        {
            for (int j = 0; j <= gridDiv; j++)
            {
                Vector2 a = Vector2.Lerp(quad[0], quad[1], (float)j / (float)gridDiv);
                Vector2 b = Vector2.Lerp(quad[1], quad[2], (float)i / (float)gridDiv);
                Vector2 b2 = Vector2.Lerp(quad[3], quad[2], (float)j / (float)gridDiv);
                Vector2 a2 = Vector2.Lerp(quad[0], quad[3], (float)i / (float)gridDiv);
                float num = Mathf.Lerp(Mathf.Lerp(vertices[3, 1], vertices[2, 1], (float)i / (float)gridDiv), Mathf.Lerp(vertices[0, 1], vertices[1, 1], (float)i / (float)gridDiv), (float)j / (float)gridDiv);
                float num2 = Mathf.Lerp(Mathf.Lerp(vertices[3, 0], vertices[2, 0], (float)i / (float)gridDiv), Mathf.Lerp(vertices[0, 0], vertices[1, 0], (float)i / (float)gridDiv), (float)j / (float)gridDiv);
                num = Mathf.Pow(num, 1f + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value) * 0.1f);
                num2 = Mathf.Pow(num2, 1f + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value) * 0.1f);
                num = Mathf.Lerp(num, UnityEngine.Random.value, 0.1f * Mathf.Pow(1f - 2f * Mathf.Abs(num - 0.1f), 2.5f));
                num2 = Mathf.Lerp(num2, UnityEngine.Random.value, 0.1f * Mathf.Pow(1f - 2f * Mathf.Abs(num - 0.3f), 2.5f));
                verts[j * (gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
                (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (gridDiv + 1) + i] = new Color(0f, 0.25f, num, alphaFade);
            }
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (washable)
        {
            if (rainIntensity > 0)
            {
                float washRate = Mathf.Lerp(0, 0.0065f, rainIntensity);
                meshDirty = false;
                for (int i = 0; i <= gridDiv; i++)
                {
                    for (int j = 0; j <= gridDiv; j++)
                    {
                        (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (gridDiv + 1) + i].a = (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (gridDiv + 1) + i].a - washRate * timeStacker;
                        if ((sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (gridDiv + 1) + i].a <= 0f)
                        {
                            base.slatedForDeletetion = true;
                        }
                    }
                }
            }
        }
        if (meshDirty)
        {
            UpdateVerts(sLeaser, rCam);
            meshDirty = false;
        }
        if (elementDirty)
        {
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("PH");
            elementDirty = false;
        }
        //0 - Bottom Left
        //1 - Top Left
        //2 - Top Right
        //3 - Bottom Right
        for (int i = 0; i < verts.Length; i++)
        {
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, verts[i] - camPos);
        }
        if (base.slatedForDeletetion || room != rCam.room)
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
