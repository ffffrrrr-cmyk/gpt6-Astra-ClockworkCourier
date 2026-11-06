using System.Collections.Generic;
using UnityEngine;

namespace ClockworkCourier
{
    public static class ArtFactory
    {
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        public static Material Brass => Material("Brushed brass", new Color(0.63f, 0.34f, 0.12f), 0.78f, 0.48f);
        public static Material Steel => Material("Blue steel", new Color(0.09f, 0.16f, 0.19f), 0.7f, 0.45f);
        public static Material Enamel => Material("Ivory enamel", new Color(0.78f, 0.76f, 0.62f), 0.25f, 0.6f);
        public static Material Dark => Material("Graphite", new Color(0.035f, 0.06f, 0.07f), 0.35f, 0.35f);
        public static Material Teal => Material("Energy glass", new Color(0.09f, 0.8f, 0.73f), 0.5f, 0.7f, 1.7f);
        public static Material Red => Material("Warning vermilion", new Color(0.95f, 0.24f, 0.10f), 0.45f, 0.5f, 0.65f);

        public static Material Material(string name, Color color, float metal = 0, float smooth = 0.4f, float emission = 0)
        {
            if (materials.TryGetValue(name, out var cached) && cached != null) return cached;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            material.SetFloat("_Metallic", metal);
            material.SetFloat("_Smoothness", smooth);
            if (metal > 0.15f)
            {
                var grain = new Texture2D(128, 128, TextureFormat.RGBA32, true) { name = name + " micro finish", wrapMode = TextureWrapMode.Repeat };
                var pixels = new Color[128 * 128];
                for (int row = 0; row < 128; row++)
                    for (int column = 0; column < 128; column++)
                    {
                        float noise = Mathf.PerlinNoise(column * 0.65f, row * 0.11f);
                        float tone = 0.87f + noise * 0.13f;
                        pixels[row * 128 + column] = new Color(tone, tone, tone, 1);
                    }
                grain.SetPixels(pixels);
                grain.Apply();
                material.SetTexture("_BaseMap", grain);
            }
            if (emission > 0)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            materials[name] = material;
            return material;
        }

        public static GameObject Part(Transform parent, string name, Vector3 position, Vector3 size, Material material, float bevel = 0.12f)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = size;
            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = BeveledBox(bevel);
            part.AddComponent<MeshRenderer>().sharedMaterial = material;
            return part;
        }

        public static GameObject Ring(Transform parent, string name, Vector3 position, float outer, float inner, float depth, Material material, int teeth = 0)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.AddComponent<MeshFilter>().sharedMesh = Annulus(outer, inner, depth, teeth);
            part.AddComponent<MeshRenderer>().sharedMaterial = material;
            return part;
        }

        public static Mesh BeveledBox(float bevel)
        {
            string key = "BeveledBox-" + bevel;
            if (meshes.TryGetValue(key, out var cached) && cached != null) return cached;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uv = new List<Vector2>();
            Vector3[] normals = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            const int subdivisions = 4;
            float radius = Mathf.Clamp(bevel, 0.01f, 0.45f);
            foreach (Vector3 normal in normals)
            {
                Vector3 tangent = Mathf.Abs(normal.y) > 0.5f ? Vector3.right : Vector3.up;
                Vector3 bitangent = Vector3.Cross(normal, tangent);
                int start = vertices.Count;
                for (int row = 0; row <= subdivisions; row++)
                    for (int column = 0; column <= subdivisions; column++)
                    {
                        Vector3 point = normal * 0.5f + tangent * ((float)column / subdivisions - 0.5f) + bitangent * ((float)row / subdivisions - 0.5f);
                        Vector3 core = new Vector3(Mathf.Clamp(point.x, -0.5f + radius, 0.5f - radius), Mathf.Clamp(point.y, -0.5f + radius, 0.5f - radius), Mathf.Clamp(point.z, -0.5f + radius, 0.5f - radius));
                        vertices.Add(core + (point - core).normalized * radius);
                        uv.Add(new Vector2((float)column / subdivisions, (float)row / subdivisions));
                        if (row == subdivisions || column == subdivisions) continue;
                        int corner = start + row * (subdivisions + 1) + column;
                        triangles.AddRange(new[] { corner, corner + 1, corner + subdivisions + 2, corner, corner + subdivisions + 2, corner + subdivisions + 1 });
                    }
            }
            var mesh = new Mesh { name = key };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uv);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshes[key] = mesh;
            return mesh;
        }

        public static Mesh Annulus(float outer, float inner, float depth, int teeth)
        {
            string key = $"Ring-{outer:F3}-{inner:F3}-{depth:F3}-{teeth}";
            if (meshes.TryGetValue(key, out var cached) && cached != null) return cached;
            int segments = teeth > 0 ? teeth * 4 : 48;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uv = new List<Vector2>();
            for (int index = 0; index < segments; index++)
            {
                float angle = index * Mathf.PI * 2 / segments;
                float nextAngle = (index + 1) * Mathf.PI * 2 / segments;
                float radius = teeth > 0 && index % 4 >= 2 ? outer * 0.88f : outer;
                float nextRadius = teeth > 0 && (index + 1) % 4 >= 2 ? outer * 0.88f : outer;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                Vector3 nextDirection = new Vector3(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle), 0);
                Vector3 front = Vector3.back * depth / 2;
                Vector3 back = Vector3.forward * depth / 2;
                Vector3 outside = direction * radius;
                Vector3 nextOutside = nextDirection * nextRadius;
                Vector3 inside = direction * inner;
                Vector3 nextInside = nextDirection * inner;
                Quad(vertices, triangles, uv, outside + front, inside + front, nextInside + front, nextOutside + front);
                Quad(vertices, triangles, uv, outside + back, nextOutside + back, nextInside + back, inside + back);
                Quad(vertices, triangles, uv, outside + front, nextOutside + front, nextOutside + back, outside + back);
                if (inner > 0) Quad(vertices, triangles, uv, inside + front, inside + back, nextInside + back, nextInside + front);
            }
            var mesh = new Mesh { name = key };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uv);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshes[key] = mesh;
            return mesh;
        }

        private static void Quad(List<Vector3> vertices, List<int> triangles, List<Vector2> uv, Vector3 first, Vector3 second, Vector3 third, Vector3 fourth)
        {
            int start = vertices.Count;
            vertices.AddRange(new[] { first, second, third, fourth });
            uv.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        public static Transform Pivot(Transform parent, string name, Vector3 position)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = position;
            return pivot;
        }

        public static MechanicalRig Courier(Transform parent, bool enemy = false, float scale = 1)
        {
            var model = Pivot(parent, "Articulated courier", Vector3.zero);
            model.localScale = Vector3.one * scale;
            Material shell = enemy ? Steel : Enamel;
            Material energy = enemy ? Red : Teal;
            var rig = model.gameObject.AddComponent<MechanicalRig>();
            rig.body = Pivot(model, "Chassis", new Vector3(0, 1.03f, 0));
            Part(rig.body, "Chest shell", Vector3.zero, new Vector3(0.64f, 0.57f, 0.43f), shell, 0.2f);
            Part(rig.body, "Underframe", new Vector3(0, -0.28f, 0), new Vector3(0.42f, 0.18f, 0.33f), Dark);
            Ring(rig.body, "Energy core surround", new Vector3(0, 0.04f, -0.25f), 0.175f, 0.117f, 0.07f, Brass, 12);
            Ring(rig.body, "Core lens", new Vector3(0, 0.04f, -0.29f), 0.111f, 0, 0.035f, energy);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(rig.body, "Shoulder bolt", new Vector3(side * 0.24f, 0.22f, -0.23f), new Vector3(0.055f, 0.055f, 0.035f), Brass);
                Part(rig.body, "Cooling vent", new Vector3(side * 0.21f, -0.13f, -0.235f), new Vector3(0.12f, 0.025f, 0.025f), Dark);
            }
            var head = Pivot(rig.body, "Head swivel", new Vector3(0, 0.48f, 0));
            Part(head, "Porcelain head", Vector3.zero, new Vector3(0.57f, 0.41f, 0.42f), shell, 0.24f);
            Part(head, "Visor gasket", new Vector3(0, 0, -0.215f), new Vector3(0.45f, 0.18f, 0.055f), Dark);
            Part(head, "Luminous visor", new Vector3(0.03f, 0, -0.249f), new Vector3(0.32f, 0.055f, 0.02f), energy);
            Part(head, "Crown brass seam", new Vector3(0, 0.21f, 0), new Vector3(0.13f, 0.035f, 0.4f), Brass);
            Part(head, "Receiver mast", new Vector3(-0.19f, 0.37f, 0), new Vector3(0.035f, 0.28f, 0.035f), Brass);
            Ring(head, "Receiver cap", new Vector3(-0.19f, 0.52f, 0), 0.055f, 0, 0.045f, energy);
            rig.leftLeg = Limb(model, "Rear leg", new Vector3(-0.19f, 0.78f, 0.09f), shell, true);
            rig.rightLeg = Limb(model, "Near leg", new Vector3(0.19f, 0.78f, -0.07f), shell, true);
            rig.leftArm = Limb(rig.body, "Rear arm", new Vector3(-0.41f, 0.17f, 0.06f), shell, false);
            rig.rightArm = Limb(rig.body, "Tool arm", new Vector3(0.41f, 0.17f, -0.05f), shell, false);
            rig.weapon = Pivot(rig.rightArm, "Winding blade", new Vector3(0.12f, -0.5f, 0));
            Part(rig.weapon, "Handle", new Vector3(0.12f, 0, 0), new Vector3(0.38f, 0.09f, 0.1f), Dark);
            Part(rig.weapon, "Guard", new Vector3(0.26f, 0, 0), new Vector3(0.07f, 0.26f, 0.16f), Brass);
            Part(rig.weapon, "Etched blade", new Vector3(0.64f, 0.015f, 0), new Vector3(0.76f, 0.14f, 0.055f), shell, 0.03f);
            Part(rig.weapon, "Powered edge", new Vector3(0.64f, -0.06f, -0.03f), new Vector3(0.72f, 0.023f, 0.025f), energy);
            Part(rig.body, "Energy parcel", new Vector3(-0.15f, 0.02f, 0.36f), new Vector3(0.4f, 0.56f, 0.3f), Brass);
            Part(rig.body, "Parcel spine", new Vector3(-0.15f, 0.02f, 0.52f), new Vector3(0.08f, 0.47f, 0.03f), energy);
            return rig;
        }

        private static Transform Limb(Transform parent, string name, Vector3 position, Material shell, bool leg)
        {
            var joint = Pivot(parent, name, position);
            Ring(joint, "Pivot bearing", Vector3.zero, 0.12f, 0.054f, 0.18f, Brass, 10);
            Part(joint, "Upper link", new Vector3(0, -0.17f, 0), new Vector3(0.17f, 0.28f, 0.2f), shell);
            Ring(joint, "Elbow bearing", new Vector3(0, -0.33f, 0), 0.09f, 0.045f, 0.2f, Steel);
            Part(joint, "Lower link", new Vector3(0, -0.46f, 0), new Vector3(0.19f, 0.24f, 0.21f), shell);
            Part(joint, leg ? "Articulated foot" : "Gripper", new Vector3(leg ? 0.07f : 0, -0.64f, -0.02f), leg ? new Vector3(0.36f, 0.15f, 0.29f) : new Vector3(0.23f, 0.18f, 0.23f), Dark);
            Part(joint, "Hydraulic rod", new Vector3(0.1f, -0.27f, 0.05f), new Vector3(0.035f, 0.38f, 0.035f), Brass);
            return joint;
        }
    }
}
