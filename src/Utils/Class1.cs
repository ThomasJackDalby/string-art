using StringArt.Tools;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Utils
{
    public static class SurfaceExtensions
    {

        public IEnumerable<Polygon2D> Collapse(this Triangle2D[] triangles)
        {
            (int Index, Triangle2D Triangle)[] indexedTriangles = triangles.Select((t, i) => (i, t)).ToArray();

            (Vector2D, (int, Triangle2D)[])[] vertexToTriangleMap = Enumerable.Empty<IGrouping<Vector2D, (int, Triangle2D)>>()
                .Concat(indexedTriangles.GroupBy(t => t.Triangle.A))
                .Concat(indexedTriangles.GroupBy(t => t.Triangle.B))
                .Concat(indexedTriangles.GroupBy(t => t.Triangle.C))
                .GroupBy(group => group.Key)
                .Select(group => (group.Key, group.SelectMany(g => g).ToArray()))
                .ToArray();

            List<(Vector2D Vertex, (int Index, Triangle2D Triangle)[] Triangles)>[] triangleToGroupMap = new List<(Vector2D Vertex, (int Index, Triangle2D Triangle)[])>[triangles.Length];
            foreach ((Vector2D Vertex, (int Index, Triangle2D Triangle)[] Triangles) vertexGroup in vertexToTriangleMap)
            {
                foreach ((int index, Triangle2D triangle) in vertexGroup.Triangles)
                {
                    triangleToGroupMap[index].Add(vertexGroup);
                }
            }

            int[] polygonIndexes = Enumerable.Range(0, triangles.Length).ToArray();
            for (int t = 0; t < indexedTriangles.Length; t++)
            {
                // get neighbours
                (int Index, Triangle2D Triangle)[] neighbours = triangleToGroupMap[t]
                    .SelectMany(t => t.Triangles)
                    .GroupBy(t => t.Index)
                    .Where(g => g.Count() == 2)
                    .Select(g => g.First())
                    .ToArray();

                foreach ((int Index, Triangle2D Triangle) in neighbours)
                {
                    if (polygonIndexes[t] == polygonIndexes[Index]) continue;
                    polygonIndexes.Replace(polygonIndexes[Index], polygonIndexes[t]);
                }
            }

            // sort into individual polygons
            (int Index, Triangle2D Triangle)[][] polygons = indexedTriangles
                .Select(t => (polygonIndexes[t.Index], t))
                .GroupBy(t => t.Item1, t => t.t)
                .Select(t => t.ToArray())
                .ToArray();

            foreach(var polygon in polygons)
            {
                // remove all edges that are paired
                Dictionary<Vector2D, Segment2D[]> pairs = polygon.SelectMany(i => i.Triangle.GetEdges())
                    .GroupBy(p => p.Start)
                    .ToDictionary(p => p.Key, p => p.ToArray());

                List<Segment2D> edges = new();
                foreach(Segment2D edge in pairs.Values)
                {
                    // if any of the values point back to the start, dismiss this edge
                    if (pairs[edge.End].Any(o => o.End == edge.Start)) continue;
                    edges.Add(edge);

                }


                // ill deal with orphaned groups later

            }
        }

        public static void Replace<T>(this T[] self, T oldValue, T newValue)
        {
            for (int i = 0; i < self.Length; i++)
            {
                if (self[i].Equals(oldValue)) self[i] = newValue;
            }
        }
    }

    public record Polygon2D();

    public record Triangle2D(Vector2D A, Vector2D B, Vector2D C)
    {

        public IEnumerable<Segment2D> GetEdges()
        {
            yield return new Segment2D(A, B);
            yield return new Segment2D(B, C);
            yield return new Segment2D(C, A);
        }
    }



    public class QuadTree2D
    {
        // want to query a single segment and see whether we need to split it

        record Node(Vector2D Centre)
        {
            private readonly Line2D vertical = new(Centre, new Vector2D(0, 1));
            private readonly Line2D horizontal = new(Centre, new Vector2D(1, 0));

            public List<Limits2D> Exclusive { get; } = new();

            private Node[]? nodes = null

            public void Add(Limits2D limits)
            {
                // if less than 10 fully inside this node, don't subdvide
                if (Exclusive.Count < 10) Exclusive.Add(limits);
                else
                {

                    // need to sub-divide
                    // if all values are the same, put in one node
                    int evaluate(Vector2D point)
                    {
                        Vector2D pp = point - Centre;
                        bool hResult = pp.Dot(horizontal.Direction) > 0;
                        bool vResult = pp.Dot(vertical.Direction) > 0;
                        return hResult ? (vResult ? 0 : 1) : (vResult ? 2 : 3);
                    }

                    int minResult = evaluate(limits.Min);
                    int maxResult = evaluate(limits.Max);

                    if (minResult == maxResult)
                    {
                        Node node = nodes[minResult] ?? (nodes[minResult] = new Node());
                        node.Add(limits);
                    }
                    else if ((minResult == 0 && maxResult == 3)
                        || (minResult == 3 && maxResult == 0)
                        || (minResult == 1 && maxResult == 2)
                        || (minResult == 2 && maxResult == 1))
                    {
                        Exclusive.Add(limits);
                    }
                    else
                    {

                    }
                }
            }

            public IEnumerable<Limits2D> Query(Limits2D limits)
            {

            }
        }

        private readonly Node root;

        public QuadTree2D(Vector2D centre)
        {
            root = new Node(centre);
        }


        public void Add(Limits2D limits)
        {


        }

        public IEnumerable<Limits2D> Query(Limits2D segment)
        {


        }
    }

    public record Segment2D(Vector2D Start, Vector2D End);

    public record Limits2D(Vector2D Min, Vector2D Max);
}