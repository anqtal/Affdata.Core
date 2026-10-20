using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Affdata.Core.Geometry
{
    public static partial class NoteMeshDefinitions
    {
        // Slide has no equivalent in project-arc; this geometry serves Affdata's Falsus-style Slides.
        // Clients sample their chart curves/timing into rows; Core owns the row interpolation,
        // triangle winding and judgement-grid topology. Shadows reuse the surface geometry.
        // Edit retains two vertices per row and duplicates rows at segment boundaries.
        // Play retains three vertices per row and shares rows within each chart piece.
        // Their sampling remains different: Edit uses >=5 ms steps capped at ~8192 segments;
        // Play uniformly divides each piece into duration/5 + 1 render segments.
        // Edit UV0/1/2 = (chart X, time), (strip X, distance), (width, total length).
        // Play UV0/1/2 = (visual travel, time), (chart X, relative floor), (strip X, width).
        // UV3/4, colours, picking state and draw batching remain client responsibilities.

        /// <summary>A sampled ribbon endpoint with the client's surface-shader UV payload.</summary>
        public readonly struct SlideVertex
        {
            public readonly Vector3 Position;
            public readonly Vector2 Uv0, Uv1, Uv2;

            public SlideVertex(Vector3 position, Vector2 uv0, Vector2 uv1, Vector2 uv2)
            {
                Position = position;
                Uv0 = uv0;
                Uv1 = uv1;
                Uv2 = uv2;
            }
        }

        /// <summary>
        /// Appends a cross-section and returns its first vertex index. UV lists may be null
        /// when building picking geometry. Reuse the caller's lists across rebuilds.
        /// </summary>
        public static int AppendSlideSurfaceRow(List<Vector3> vertices,
            List<Vector2> uv0, List<Vector2> uv1, List<Vector2> uv2,
            SlideVertex first, SlideVertex last, int verticesPerRow)
        {
            if (verticesPerRow < 2) throw new ArgumentOutOfRangeException(nameof(verticesPerRow));
            int start = vertices.Count;
            for (int i = 0; i < verticesPerRow; i++)
            {
                float t = i / (float)(verticesPerRow - 1);
                if (i == 0) Add(first);
                else if (i == verticesPerRow - 1) Add(last);
                else Add(new SlideVertex(
                    Vector3.LerpUnclamped(first.Position, last.Position, t),
                    Vector2.LerpUnclamped(first.Uv0, last.Uv0, t),
                    Vector2.LerpUnclamped(first.Uv1, last.Uv1, t),
                    Vector2.LerpUnclamped(first.Uv2, last.Uv2, t)));
            }
            return start;

            void Add(SlideVertex vertex)
            {
                vertices.Add(vertex.Position);
                uv0?.Add(vertex.Uv0);
                uv1?.Add(vertex.Uv1);
                uv2?.Add(vertex.Uv2);
            }
        }

        /// <summary>Joins two equally sized rows; call separately for disconnected chart pieces.</summary>
        public static void AppendSlideSurfaceSegment(List<int> indices,
            int firstRow, int nextRow, int verticesPerRow)
        {
            if (verticesPerRow < 2) throw new ArgumentOutOfRangeException(nameof(verticesPerRow));
            for (int column = 0; column < verticesPerRow - 1; column++)
            {
                int a = firstRow + column, b = nextRow + column;
                indices.Add(a); indices.Add(b); indices.Add(a + 1);
                indices.Add(a + 1); indices.Add(b); indices.Add(b + 1);
            }
        }

        /// <summary>
        /// Creates the staggered Slide judgement grid. Each triangle has three unshared
        /// vertices and identity indices: SV_VertexID / 3 and UV1.x both identify the cell.
        /// UV0 is the unshifted grid position; UV1.y is the row number.
        /// </summary>
        public static Mesh CreateSlideJudgementGrid(int rows, int columns,
            Vector2 positionOffset, int horizontalSpan = 1)
        {
            // Both clients use the Falsus-style alternating triangle rows. Edit uses a
            // zero offset and scales coverage in its shader; Play centres the mesh
            // with (-.5,-.5), optionally extending columns without stretching existing cells.
            // Density, world scale, opacity animation, blur and render targets stay local.
            rows = Mathf.Max(1, rows);
            columns = Mathf.Max(1, columns);
            horizontalSpan = Mathf.Max(1, horizontalSpan);
            int extendedColumns = checked(columns * horizontalSpan);
            int triangleCount = checked(rows * (extendedColumns * 2 + 1));
            int vertexCount = checked(triangleCount * 3);
            var vertices = new List<Vector3>(vertexCount);
            var uvs = new List<Vector2>(vertexCount);
            var cellData = new List<Vector2>(vertexCount);
            var indices = new List<int>(vertexCount);
            float columnWidth = 1f / columns;
            float startX = (1 - horizontalSpan) * .5f;
            float rowHeight = 1f / rows;
            int cellId = 0;

            for (int row = 0; row < rows; row++)
            {
                float top = 1f - row * rowHeight;
                float bottom = top - rowHeight;
                if ((row & 1) == 0)
                {
                    float topX = startX, bottomX = startX + columnWidth * .5f;
                    Triangle(new Vector2(topX, top), new Vector2(bottomX, bottom),
                        new Vector2(topX + columnWidth, top), row);
                    topX += columnWidth;
                    for (int column = 0; column < extendedColumns; column++)
                    {
                        Triangle(new Vector2(bottomX, bottom), new Vector2(topX, top),
                            new Vector2(bottomX + columnWidth, bottom), row);
                        bottomX += columnWidth;
                        Triangle(new Vector2(topX, top), new Vector2(bottomX, bottom),
                            new Vector2(topX + columnWidth, top), row);
                        topX += columnWidth;
                    }
                }
                else
                {
                    float topX = startX + columnWidth * .5f, bottomX = startX;
                    Triangle(new Vector2(bottomX, bottom), new Vector2(topX, top),
                        new Vector2(bottomX + columnWidth, bottom), row);
                    bottomX += columnWidth;
                    for (int column = 0; column < extendedColumns; column++)
                    {
                        Triangle(new Vector2(topX, top), new Vector2(bottomX, bottom),
                            new Vector2(topX + columnWidth, top), row);
                        topX += columnWidth;
                        Triangle(new Vector2(bottomX, bottom), new Vector2(topX, top),
                            new Vector2(bottomX + columnWidth, bottom), row);
                        bottomX += columnWidth;
                    }
                }
            }

            var mesh = new Mesh
            {
                name = "Slide judgement grid",
                indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16,
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(1, cellData);
            mesh.SetTriangles(indices, 0, true);
            return mesh;

            void Triangle(Vector2 a, Vector2 b, Vector2 c, int row)
            {
                Vertex(a, row); Vertex(b, row); Vertex(c, row);
                cellId++;
            }

            void Vertex(Vector2 uv, int row)
            {
                indices.Add(vertices.Count);
                vertices.Add(new Vector3(uv.x + positionOffset.x, uv.y + positionOffset.y, 0));
                uvs.Add(uv);
                cellData.Add(new Vector2(cellId, row));
            }
        }
    }
}
