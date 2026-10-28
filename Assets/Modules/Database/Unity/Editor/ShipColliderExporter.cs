using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CombatSimulation.Physics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using BigInteger = System.Numerics.BigInteger;

namespace GameDatabase.Editor
{
    // Writes Ship.Collider (phase 4, D5) from the root colliders of each ship prefab, at prefab scale 1.
    // 1 point is a circle, 2 points a capsule, 3 or more an outline. Ships that share a model get the same data;
    // a ship without a prefab keeps an empty field. Running it again replaces the field.
    public static class ShipColliderExporter
    {
        private const string ShipFolder = "Assets/Modules/Database/Resources/Database/Ship";
        private const string PrefabFolder = "Assets/Modules/BattleSimulator/Resources/Combat/Ships";
        private const string ShipSchema = "Assets/Modules/Database/.Schema/v1/Objects/Ship.xml";
        private const string FieldName = "Collider";

        [MenuItem("Tools/Database/Export Ship Colliders")]
        public static void Export()
        {
            var fieldsBefore = FieldsBefore(FieldName);
            var report = new StringBuilder();
            int written = 0, unchanged = 0, failed = 0;

            foreach (var path in Directory.GetFiles(ShipFolder, "*.json").OrderBy(p => p, StringComparer.Ordinal))
            {
                try
                {
                    // Git stores the files with LF; a checkout may have CRLF.
                    var original = File.ReadAllText(path).Replace("\r\n", "\n");
                    var json = Parse(original);
                    if (Serialize(json) != original)
                        throw new InvalidOperationException("the file does not round-trip through the JSON writer");

                    var image = (string)json["ModelImage"];
                    var prefab = string.IsNullOrEmpty(image) ? null : AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{image}.prefab");
                    if (prefab == null)
                    {
                        report.AppendLine($"{Path.GetFileName(path)}: no prefab '{image}', field left empty");
                        continue;
                    }

                    var shapes = ReadShapes(prefab, out var skippedPaths, out var holes);
                    SetField(json, shapes, fieldsBefore);

                    var text = Serialize(json);
                    if (text == original)
                    {
                        ++unchanged;
                        continue;
                    }

                    File.WriteAllText(path, text, new UTF8Encoding(false));
                    ++written;
                    report.AppendLine($"{Path.GetFileName(path)} ({image}): {shapes.Count} shapes" +
                        (skippedPaths > 0 ? $", {skippedPaths} short paths left out" : string.Empty) +
                        (holes > 0 ? $", {holes} holes left out" : string.Empty));
                }
                catch (Exception e)
                {
                    ++failed;
                    report.AppendLine($"{Path.GetFileName(path)}: ERROR {e.Message}");
                }
            }

            AssetDatabase.Refresh();
            var summary = $"ShipColliderExporter: {written} written, {unchanged} unchanged, {failed} failed.\n{report}";
            if (failed > 0) Debug.LogError(summary);
            else Debug.Log(summary);
        }

        private static List<JObject> ReadShapes(GameObject prefab, out int skippedPaths, out int holes)
        {
            skippedPaths = 0;
            holes = 0;

            var root = prefab.transform;
            if (root.localScale != Vector3.one || root.localRotation != Quaternion.identity)
                throw new InvalidOperationException("the prefab root is scaled or rotated");

            var colliders = prefab.GetComponents<Collider2D>();
            if (prefab.GetComponentsInChildren<Collider2D>(true).Length != colliders.Length)
                throw new InvalidOperationException("a child object has a collider");

            var shapes = new List<JObject>();
            foreach (var collider in colliders)
            {
                if (!collider.enabled || collider.isTrigger) continue;
                var offset = collider.offset;

                switch (collider)
                {
                    case PolygonCollider2D polygon:
                        var paths = new List<Vec2[]>();
                        for (var i = 0; i < polygon.pathCount; ++i)
                        {
                            // Unity makes no shape from a path with fewer than 3 points.
                            var points = polygon.GetPath(i);
                            if (points.Length < 3)
                            {
                                ++skippedPaths;
                                continue;
                            }

                            paths.Add(points.Select(p => new Vec2(p.x + offset.x, p.y + offset.y)).ToArray());
                        }

                        for (var i = 0; i < paths.Count; ++i)
                        {
                            if (ConvexDecomposition.IsHole(paths, i))
                            {
                                ++holes;
                                continue;
                            }

                            shapes.Add(Shape(paths[i], 0f));
                        }
                        break;

                    case CircleCollider2D circle:
                        shapes.Add(Shape(new[] { new Vec2(offset.x, offset.y) }, circle.radius));
                        break;

                    case CapsuleCollider2D capsule:
                        var vertical = capsule.direction == CapsuleDirection2D.Vertical;
                        var size = capsule.size;
                        var across = vertical ? size.x : size.y;
                        var along = vertical ? size.y : size.x;
                        var center = new Vec2(offset.x, offset.y);
                        if (!(along > across))
                        {
                            // Equal sides make a circle.
                            shapes.Add(Shape(new[] { center }, 0.5f * across));
                            break;
                        }

                        var core = Capsule.FromSize(center, size.x, size.y, vertical);
                        shapes.Add(Shape(new[] { core.Center1, core.Center2 }, core.Radius));
                        break;

                    case BoxCollider2D box:
                        if (box.edgeRadius > 0f)
                            throw new InvalidOperationException("a box collider has an edge radius");

                        var halfX = 0.5f * box.size.x;
                        var halfY = 0.5f * box.size.y;
                        shapes.Add(Shape(new[]
                        {
                            new Vec2(offset.x - halfX, offset.y - halfY),
                            new Vec2(offset.x + halfX, offset.y - halfY),
                            new Vec2(offset.x + halfX, offset.y + halfY),
                            new Vec2(offset.x - halfX, offset.y + halfY),
                        }, 0f));
                        break;

                    default:
                        throw new InvalidOperationException($"unsupported collider {collider.GetType().Name}");
                }
            }

            return shapes;
        }

        // Matches the Database Editor output: zero coordinates and a zero radius are left out.
        private static JObject Shape(IReadOnlyList<Vec2> points, float radius)
        {
            var array = new JArray();
            foreach (var point in points)
            {
                var item = new JObject();
                if (point.X != 0f) item["x"] = Number(point.X);
                if (point.Y != 0f) item["y"] = Number(point.Y);
                array.Add(item);
            }

            var shape = new JObject { ["Points"] = array };
            if (radius != 0f) shape["Radius"] = Number(radius);
            return shape;
        }

        // The Database Editor (.NET) writes a float as the shortest text that reads back as the same value, with ties
        // between two closest candidates going to the even digit. Unity's Mono formats differently (9 digits for "R",
        // ties away from zero), so this builds the editor's text from the exact binary value.
        private static JToken Number(float value) => new JRaw(Shortest(value));

        private static string Shortest(float value)
        {
            if (value == 0f) return "0.0";

            var bits = BitConverter.SingleToInt32Bits(value);
            var biased = (bits >> 23) & 0xFF;
            var mantissa = new BigInteger(bits & 0x7FFFFF);
            var exponent = -149;
            if (biased != 0)
            {
                mantissa += 1 << 23;
                exponent = biased - 150;
            }

            var estimate = (int)Math.Floor(Math.Log10(Math.Abs((double)value)));
            for (var precision = 1; precision <= 9; ++precision)
            {
                var digits = RoundToDigits(mantissa, exponent, precision, estimate, out var power);
                var text = (value < 0f ? "-" : string.Empty) + Format(digits.ToString(CultureInfo.InvariantCulture), power);
                if (float.Parse(text, CultureInfo.InvariantCulture) == value) return text;
            }

            throw new InvalidOperationException($"no round-trip text for {value:R}");
        }

        // |mantissa × 2^exponent| rounded half to even to 'precision' digits. 'power' is the decimal exponent of the first digit.
        private static BigInteger RoundToDigits(BigInteger mantissa, int exponent, int precision, int power0, out int power)
        {
            power = power0;
            var lower = BigInteger.Pow(10, precision - 1);
            while (true)
            {
                var shift = precision - 1 - power;
                var numerator = exponent >= 0 ? mantissa << exponent : mantissa;
                var denominator = exponent >= 0 ? BigInteger.One : BigInteger.One << -exponent;
                if (shift >= 0) numerator *= BigInteger.Pow(10, shift);
                else denominator *= BigInteger.Pow(10, -shift);

                var digits = BigInteger.DivRem(numerator, denominator, out var remainder);
                var twice = remainder * 2;
                if (twice > denominator || (twice == denominator && !digits.IsEven)) ++digits;

                if (digits < lower) { --power; continue; }
                if (digits == lower * 10) { ++power; return lower; }
                if (digits > lower * 10) { ++power; continue; }
                return digits;
            }
        }

        // .NET layout: an exponent below 1e-4 ("2.6382452E-10"), plain digits otherwise, always with a decimal point.
        private static string Format(string digits, int power)
        {
            digits = digits.TrimEnd('0');
            if (power < -4)
                return digits.Substring(0, 1) + (digits.Length > 1 ? "." + digits.Substring(1) : string.Empty) +
                    "E-" + (-power).ToString("00", CultureInfo.InvariantCulture);
            if (power < 0)
                return "0." + new string('0', -power - 1) + digits;

            var whole = digits.Length > power + 1 ? digits.Substring(0, power + 1) : digits.PadRight(power + 1, '0');
            var fraction = digits.Length > power + 1 ? digits.Substring(power + 1) : "0";
            return whole + "." + fraction;
        }

        // Puts the field where the Database Editor writes it: after the last present field that comes first in the schema.
        private static void SetField(JObject json, List<JObject> shapes, HashSet<string> fieldsBefore)
        {
            json.Remove(FieldName);
            if (shapes.Count == 0) return;

            var property = new JProperty(FieldName, new JArray(shapes));
            var anchor = json.Properties().LastOrDefault(p => fieldsBefore.Contains(p.Name));
            if (anchor != null) anchor.AddAfterSelf(property);
            else json.AddFirst(property);
        }

        private static HashSet<string> FieldsBefore(string field)
        {
            var names = new HashSet<string> { "ItemType", "Id" };
            foreach (var member in XDocument.Load(ShipSchema).Root.Elements("member"))
            {
                var name = (string)member.Attribute("name");
                if (name == field) return names;
                names.Add(name);
            }

            throw new InvalidOperationException($"{ShipSchema} has no member {field}");
        }

        // Decimal keeps the digits of each number as written, so unchanged fields write back unchanged. Collider
        // numbers are floats written in their shortest form (by this tool or the Database Editor); read them back as floats.
        private static JObject Parse(string text)
        {
            using var reader = new JsonTextReader(new StringReader(text)) { FloatParseHandling = FloatParseHandling.Decimal };
            var json = JObject.Load(reader);
            if (json[FieldName] is JToken collider)
                foreach (var value in collider.SelectTokens("..*").OfType<JValue>().Where(v => v.Type == JTokenType.Float).ToList())
                    value.Replace(Number(float.Parse(((decimal)value.Value).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)));

            return json;
        }

        private static string Serialize(JObject json)
        {
            using var text = new StringWriter { NewLine = "\n" };
            using (var writer = new JsonTextWriter(text) { Formatting = Formatting.Indented, Indentation = 2 })
                json.WriteTo(writer);
            return text.ToString();
        }
    }
}
