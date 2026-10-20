using System.Collections.Generic;
using UnityEngine;

namespace Affdata.Core.Geometry
{
    /// <summary>Shared note geometry. Every factory returns a new mesh owned by its caller.</summary>
    public static partial class NoteMeshDefinitions
    {
        // Compatibility notes below compare against project-arc at 1/100 of its world units.
        // Reference paths are relative to project-arc. Unless stated otherwise, keep the
        // existing Affdata appearance; sharing a mesh factory does not also unify cameras,
        // materials, sorting, clipping or per-note transforms.
        // Edit uses CreateSprite for skinned planes; Play uses the explicit plane factories.
        // Chart-dependent Slide surfaces, render batching and editor picking stay in clients.
        public const float SfxArcTapScale = 4.2f;

        // Ordinary ArcTap retains the Affdata centre at y = -.5. project-arc combines
        // RenderArcNote::placeArcTaps (-50) and RenderArcTapNote::init (+5), giving -.45.
        // SFX ArcTap already uses that reference centre and its 420/100 model scale.
        public static readonly Vector3 ArcTapCenter = new Vector3(0, -.5f, -.6f);
        public static readonly Vector3 SfxArcTapCenter = new Vector3(0, -.45f, -.6f);

        // Planar in both implementations. This template uses y = 0; project-arc's
        // Classes/game/render/ArcSegment.cpp::updateQuads projects the shadow to y = .05.
        // Slam-shadow deformation and visibility remain caller responsibilities.
        public static Mesh CreateArcShadow() => Make("Arc shadow",
            new[] { new Vector3(-1,0,0), new Vector3(-1,0,1), new Vector3(1,0,1), new Vector3(1,0,0) },
            new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right }, new[] { 0,1,2, 0,2,3 });

        // Same two visible triangles as ArcSegment.cpp's collapsed head-cap quads.
        // Retain Affdata's UV seams: Edit uses these UVs, Play flips U and V.
        // Trace head height retains the .15 versus .14 difference documented below.
        public static Mesh CreateArcHead() => Make("Arc head",
            new[] { new Vector3(0,.5f,0), new Vector3(1,-.5f,0), new Vector3(0,-.5f,1), new Vector3(-1,-.5f,0) },
            new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.one }, new[] { 0,2,1, 0,3,2 });

        // Bottom-centred floor quad. UV orientation matches Play's imported Tap/Hold models.
        // Classes/game/render/RenderNote.cpp uses Tap width 3.82 and Hold width 3.825.
        // Play retains 3.8; Edit's default Sprite width is 247/100 * 1.53 = 3.7791.
        // Tap length is also not unified: project-arc uses trunc(200 + 340*k)/100,
        // with k depending on distance and speed. Play uses CalculateTapSizeScalar;
        // Edit uses ArcTapNoteManager's Sprite-height multiplier and editor-camera override.
        // Hold length/clipping remains runtime state, not part of this unit-depth template.
        // Play also uses this factory for the 4.25 x 1.2 ArcTap shadow at y = 0;
        // project-arc puts that shadow at y = .06 and applies its speedFactor to depth.
        public static Mesh CreateGroundNote(float width, float depth) => Make("Ground note",
            new[] { new Vector3(width/2,0,0), new Vector3(width/2,0,-depth),
                new Vector3(-width/2,0,-depth), new Vector3(-width/2,0,0) },
            new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right }, new[] { 0,1,2, 0,2,3 });

        // With default skins both clients retain width .51 * 2.34 = 1.1934 and floor y = 0.
        // ArcSegment.cpp instead insets .3 from each side of the 1.8-wide Arc:
        // width 1.2, from floor y = .05 to the bottom edge of the Arc cross-section.
        public static Mesh CreateHeightIndicator() => Make("Height indicator",
            new[] { new Vector3(.5f,-1,0), new Vector3(.5f,0,0), new Vector3(-.5f,0,0), new Vector3(-.5f,-1,0) },
            new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right }, new[] { 0,1,2, 0,2,3 });

        // Retain Affdata's 2.56-square base, also used by Edit's normalized Sprite.
        // project-arc loads img/1080/arc_cap.png (84 px); TrackLayer.cpp::updateArcCaps
        // scales it by (base + shrink*.5)*4.5. Affdata applies base/shrink directly
        // to this 2.56-square base, so those source size/scale pairs are not equivalent.
        // Cap visibility and approach/fade logic remain in the clients; screen size also
        // depends on each camera.
        public static Mesh CreateArcCap() => Make("Arc cap",
            new[] { new Vector3(1.28f,-1.28f,0), new Vector3(1.28f,1.28f,0),
                new Vector3(-1.28f,1.28f,0), new Vector3(-1.28f,-1.28f,0) },
            new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right }, new[] { 0,1,2, 0,2,3 });

        /// <summary>Adapts a newly created mesh to a caller's coordinates and texture orientation.</summary>
        public static Mesh Transform(Mesh mesh, Vector3 scale, Vector3 offset,
            Vector2 uvScale, Vector2 uvOffset)
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = mesh.uv;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.Scale(vertices[i], scale) + offset;
                uv[i] = Vector2.Scale(uv[i], uvScale) + uvOffset;
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            if (scale.x * scale.y * scale.z < 0)
            {
                for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
                {
                    int[] indices = mesh.GetTriangles(submesh);
                    for (int i = 0; i < indices.Length; i += 3)
                    { int swap = indices[i+1]; indices[i+1] = indices[i+2]; indices[i+2] = swap; }
                    mesh.SetTriangles(indices, submesh);
                }
            }
            mesh.RecalculateBounds();
            return mesh;
        }

        // ArcSegment.cpp::crossSection: solid Arc width/height = 1.8/.9, matching
        // this template at scale .9. Both clients scale Trace by .15: width/height = .3/.15.
        // project-arc uses integer 30/4 = 7 for Trace's half-height, giving .3/.14.
        // Keep the existing difference. Edit uses these UVs and Play flips U; project-arc
        // assigns sprite UVs separately to faceA/faceB, so UV layout is not a shared baseline.
        public static Mesh CreateArcSegment()
        {
            var mesh = Make("Arc segment",
                new[] { new Vector3(0,.5f,0), new Vector3(0,.5f,1), new Vector3(1,-.5f,1),
                    new Vector3(1,-.5f,0), new Vector3(-1,-.5f,1), new Vector3(-1,-.5f,0) },
                new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right, Vector2.one, Vector2.right },
                new[] { 0,3,2, 0,2,1, 0,5,4, 0,4,1 });
            // Submesh 0 retains the complete trace; solid Arcs sort the two faces independently.
            mesh.subMeshCount = 3;
            mesh.SetTriangles(new[] { 0,3,2, 0,2,1 }, 1);
            mesh.SetTriangles(new[] { 0,5,4, 0,4,1 }, 2);
            return mesh;
        }

        public static Mesh CreateConnectionLine()
        {
            // TrackLayer.cpp::createArcTapLinks sets a 15-unit-wide planar sprite.
            // At 1/100 world scale the shared width is .15 (previously .075).
            // This aligns geometry only; client tint/alpha and group transforms remain local.
            const float halfWidth = 0.075f;
            return Make("Note connection",
                new[] { new Vector3(-halfWidth, 0, 0), new Vector3(-halfWidth, 0, 1),
                    new Vector3(halfWidth, 0, 1), new Vector3(halfWidth, 0, 0) },
                new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right },
                new[] { 0, 1, 2, 0, 2, 3 });
        }

        public static Mesh CreateSprite(Sprite sprite)
        {
            // Preserve a skin's pivot, tight outline and atlas UVs.
            Vector2[] source = sprite.vertices;
            var vertices = new Vector3[source.Length];
            for (int i = 0; i < source.Length; i++) vertices[i] = source[i];
            ushort[] sourceIndices = sprite.triangles;
            var indices = new int[sourceIndices.Length];
            for (int i = 0; i < indices.Length; i++) indices[i] = sourceIndices[i];
            return Make("Note sprite " + sprite.name, vertices, sprite.uv, indices);
        }

        private static Mesh Make(string name, Vector3[] vertices, Vector2[] uv, int[] indices)
        {
            var mesh = new Mesh { name = name, vertices = vertices, uv = uv, triangles = indices };
            mesh.colors = System.Array.ConvertAll(vertices, _ => Color.white);
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateArcTap()
        {
            var b = new Builder();
            // Retain the original cube's face orientation and UV seams.
            // assets/models/tap_l.obj is also a unit cube; after caller transforms both
            // render at 4.4625 x .6 x 1.2 for width = 1. Play applies 4.25 then 1.05 in X.
            // Per-face UVs differ from that OBJ despite matching dimensions; preserve the
            // existing Affdata mapping. The centre-height difference is documented above.
            var p = new[] { new Vector3(.5f,-.5f,.5f), new Vector3(-.5f,-.5f,.5f),
                new Vector3(.5f,.5f,.5f), new Vector3(-.5f,.5f,.5f), new Vector3(.5f,.5f,-.5f),
                new Vector3(-.5f,.5f,-.5f), new Vector3(.5f,-.5f,-.5f), new Vector3(-.5f,-.5f,-.5f) };
            b.Face(new[] { p[0],p[2],p[3],p[1] }, new[] { Vector2.zero,Vector2.up,Vector2.one,Vector2.right });
            b.Face(new[] { p[2],p[4],p[5],p[3] }, new[] { Vector2.zero,Vector2.up,Vector2.one,Vector2.right });
            b.Face(new[] { p[4],p[6],p[7],p[5] }, new[] { Vector2.zero,Vector2.up,Vector2.one,Vector2.right });
            b.Face(new[] { p[6],p[0],p[1],p[7] });
            b.Face(new[] { p[1],p[3],p[5],p[7] });
            b.Face(new[] { p[6],p[4],p[2],p[0] });
            return b.Build("ArcTap cube");
        }

        // Shared Affdata Slide extension. project-arc has no corresponding Slide/bracket
        // implementation to match; retain the existing outline, thickness and atlas UVs.
        public static Mesh CreateSlideBracket()
        {
            var b = new Builder();
            var outline = new[] { new Vector2(-.00040f,-.03536f), new Vector2(-.03698f,-.03536f),
                new Vector2(-.03698f,.03536f), new Vector2(-.00040f,.03536f), new Vector2(-.00040f,.02121f),
                new Vector2(-.02284f,.02121f), new Vector2(-.02284f,-.02121f), new Vector2(-.00040f,-.02121f) };
            var uv = new Vector2[8];
            for (int i = 0; i < 8; i++) uv[i] = new Vector2(1 + (outline[i].x + .0004f) / .08755f, .5f + outline[i].y / .08755f);
            var backUV = new Vector2[8];
            for (int i = 0; i < 8; i++) backUV[i] = new Vector2(1.0822f-uv[i].x,uv[i].y);
            b.Prism(outline, -.00339f, .00381f, uv, new[] { 0,1,7, 7,1,6, 1,5,6, 2,5,1, 3,5,2, 4,5,3 }, backUV,
                new[] {
                    UV(.0822f,.9038f, .0822f,.986f, .5f,.986f, .5f,.9038f),
                    UV(.0822f,.0962f, 0,.0962f, 0,.9038f, .0822f,.9038f),
                    UV(.5f,.0962f, .5f,.014f, .0822f,.014f, .0822f,.0962f),
                    UV(.5f,.9038f, .5822f,.9038f, .5822f,.7423f, .5f,.7423f),
                    UV(.0822f,.9038f, .0822f,.986f, .3385f,.986f, .3385f,.9038f),
                    UV(.5f,.7423f, .5822f,.7423f, .5822f,.2577f, .5f,.2577f),
                    UV(.3385f,.0962f, .3385f,.014f, .0822f,.014f, .0822f,.0962f),
                    UV(.5f,.2577f, .5822f,.2577f, .5822f,.0962f, .5f,.0962f)
                });
            return b.Build("Slide bracket");
        }

        private static Vector2[] UV(float u0, float v0, float u1, float v1, float u2, float v2, float u3, float v3)
            => new[] { new Vector2(u0,v0),new Vector2(u1,v1),new Vector2(u2,v2),new Vector2(u3,v3) };

        private sealed class Builder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector2> uv = new List<Vector2>();
            public readonly List<int> Indices = new List<int>();
            public void Face(Vector3[] points, Vector2[] coords = null)
            {
                int start = vertices.Count;
                vertices.AddRange(points);
                uv.AddRange(coords ?? new[] { Vector2.zero,Vector2.up,Vector2.one,Vector2.right });
                Indices.AddRange(new[] { start,start+1,start+2, start,start+2,start+3 });
            }
            public void Prism(Vector2[] outline, float back, float front, Vector2[] coords, int[] triangles,
                Vector2[] backCoords = null, Vector2[][] sideCoords = null)
            {
                Vector2 a = outline[triangles[1]] - outline[triangles[0]], c = outline[triangles[2]] - outline[triangles[0]];
                bool clockwise = a.x*c.y - a.y*c.x < 0;
                for (int face = 0; face < 2; face++)
                {
                    int start = vertices.Count;
                    foreach (var p in outline) vertices.Add(new Vector3(p.x, p.y, face == 0 ? front : back));
                    uv.AddRange(face == 0 ? coords : backCoords ?? coords);
                    bool reverse = clockwise ^ (face == 1);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        Indices.Add(start + triangles[i]);
                        Indices.Add(start + triangles[i + (reverse ? 2 : 1)]);
                        Indices.Add(start + triangles[i + (reverse ? 1 : 2)]);
                    }
                }
                for (int i = 0; i < outline.Length; i++)
                {
                    Vector2 p = outline[i], q = outline[(i + 1) % outline.Length];
                    Face(new[] { new Vector3(p.x,p.y,front),new Vector3(p.x,p.y,back),
                        new Vector3(q.x,q.y,back),new Vector3(q.x,q.y,front) }, sideCoords?[i]);
                    if (clockwise)
                        for (int t = Indices.Count - 6; t < Indices.Count; t += 3)
                        { int swap = Indices[t+1]; Indices[t+1] = Indices[t+2]; Indices[t+2] = swap; }
                }
            }
            public Mesh Build(string name) => Make(name, vertices.ToArray(), uv.ToArray(), Indices.ToArray());
        }

    }
}
