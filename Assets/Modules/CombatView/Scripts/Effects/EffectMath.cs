using UnityEngine;

namespace CombatView.Effects
{
    // The helpers the copied effect code took from CommonComponents (RotationHelpers, Geometry, Primitives and the
    // random extensions); this module does not reference it. One random source for every effect: presentation only, so
    // it never touches UnityEngine.Random or the battle seed.
    internal static class EffectMath
    {
        public static readonly System.Random Random = new System.Random();

        public static float NextFloat() => (float)Random.NextDouble();
        public static float NextFloatSigned() => 2f * (float)Random.NextDouble() - 1f;
        public static float NextFloatSigned(System.Random random) => 2f * (float)random.NextDouble() - 1f;

        // Legacy RotationHelpers.Direction, without its lookup table.
        public static Vector2 Direction(float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        // Legacy RotationHelpers.Transform: the point turned by the angle in degrees.
        public static Vector2 Rotate(Vector2 point, float degrees)
        {
            var direction = Direction(degrees);
            return new Vector2(point.x * direction.x - point.y * direction.y, point.y * direction.x + point.x * direction.y);
        }

        // Legacy Geometry.Bezier: a quadratic curve.
        public static Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float t) =>
            (1 - t) * (1 - t) * start + 2 * t * (1 - t) * control + t * t * end;

        // Legacy GameObjectExtensions.Move: the local position without its z.
        public static void Move(Transform transform, Vector2 position)
        {
            var local = transform.localPosition;
            local.x = position.x;
            local.y = position.y;
            transform.localPosition = local;
        }

        // Legacy Primitives.CreateCircle: a ring of the given thickness, the texture repeated along it.
        public static void CreateCircle(Mesh mesh, float radius, float thickness, int segments, float textureAspect)
        {
            mesh.Clear(false);

            var vertices = new Vector3[2 * (segments + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[6 * segments];

            var length = 2 * Mathf.PI * radius;
            var step = Mathf.Round(textureAspect * length / thickness) / segments;
            var inner = 1.0f - thickness / radius;

            for (var i = 0; i <= segments; ++i)
            {
                var alpha = i * 2 * Mathf.PI / segments;
                var x = Mathf.Cos(alpha);
                var y = Mathf.Sin(alpha);
                vertices[2 * i] = new Vector3(x, y, 0);
                vertices[2 * i + 1] = new Vector3(inner * x, inner * y, 0);
                uv[2 * i] = new Vector2(i * step, 1);
                uv[2 * i + 1] = new Vector2(i * step, 0);
            }

            for (var i = 0; i < segments; ++i)
            {
                var p1 = i * 2;
                var p2 = i * 2 + 1;
                var p3 = i * 2 + 2;
                var p4 = i * 2 + 3;
                var index = 6 * i;
                triangles[index] = p2;
                triangles[index + 1] = p1;
                triangles[index + 2] = p3;
                triangles[index + 3] = p3;
                triangles[index + 4] = p4;
                triangles[index + 5] = p2;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
        }

        // A texture on a line renderer without a new material: the material stays shared, as the legacy template's.
        public static void SetTexture(Renderer renderer, Texture texture, MaterialPropertyBlock block)
        {
            renderer.GetPropertyBlock(block);
            block.SetTexture(MainTex, texture ? texture : Texture2D.whiteTexture);
            renderer.SetPropertyBlock(block);
        }

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    }
}
