using UnityEngine;
using UnityEngine.Rendering;

namespace ClockworkCourier
{
    public static class WorldBuilder
    {
        public static GameObject Build(ProgressData progress)
        {
            var root = new GameObject("World");
            Ground(root.transform, 9, 18);
            Ground(root.transform, 33, 22);
            Ground(root.transform, 61, 26);
            Ground(root.transform, 85.5f, 15);
            Ground(root.transform, 108, 22);
            Ground(root.transform, 135, 24);
            Ground(root.transform, 161.5f, 21);
            Ground(root.transform, 195.5f, 39);
            Solid(root.transform, "Western boundary", new Vector3(-1.5f, 3, 0), new Vector3(2, 8, 3), ArtFactory.Steel);
            Solid(root.transform, "Eastern boundary", new Vector3(216, 3, 0), new Vector3(2, 8, 3), ArtFactory.Steel);
            for (int index = 0; index < 6; index++) Checkpoint(root.transform, index, Campaign.CheckpointX[index]);
            float[] pits = { 20, 46, 76, 95, 121, 149, 174 };
            foreach (float gap in pits)
            {
                var bridge = Platform(root.transform, "Counterweight lift", new Vector3(gap, -0.05f, 0), 2.8f);
                var mover = bridge.AddComponent<MovingPlatform>();
                mover.travel = new Vector3(0, gap > 80 ? 2.8f : 1.4f, 0);
                mover.period = 4.8f;
                ArtFactory.Part(root.transform, "Lift chain rail", new Vector3(gap, 2, 1.3f), new Vector3(0.07f, 6, 0.07f), ArtFactory.Brass);
            }
            Platform(root.transform, "Workshop step", new Vector3(13, 0.8f, 0), 3);
            Platform(root.transform, "Workshop overlook", new Vector3(16.5f, 2.0f, 0), 3);
            Platform(root.transform, "West return ledge", new Vector3(40, 3.4f, 0), 5);
            Platform(root.transform, "West shortcut walkway", new Vector3(47, 4.7f, 0), 8);
            Platform(root.transform, "West walkway descent", new Vector3(53, 3.4f, 0), 4);
            if (progress.shortcutWest) ShortcutBridge(root.transform, 46);
            else Solid(root.transform, "West shortcut gate", new Vector3(46, 0.7f, 1.6f), new Vector3(4.4f, 0.12f, 0.15f), ArtFactory.Red);
            Interactable(root.transform, WorldInteractable.Kind.ShortcutWest, new Vector3(51, 4.95f, 0), "west-lever");
            Platform(root.transform, "Lift access step", new Vector3(82, 1.3f, 0), 3.5f);
            Platform(root.transform, "Lift mezzanine", new Vector3(87, 2.9f, 0), 5);
            Platform(root.transform, "Lift second step", new Vector3(91, 4.3f, 0), 3);
            Platform(root.transform, "East return ledge", new Vector3(115, 3.5f, 0), 6);
            Platform(root.transform, "East shortcut walkway", new Vector3(122, 4.7f, 0), 8);
            Platform(root.transform, "East walkway descent", new Vector3(128, 3.3f, 0), 4);
            if (progress.shortcutEast) ShortcutBridge(root.transform, 121);
            else Solid(root.transform, "East shortcut gate", new Vector3(121, 0.7f, 1.6f), new Vector3(4.4f, 0.12f, 0.15f), ArtFactory.Red);
            Interactable(root.transform, WorldInteractable.Kind.ShortcutEast, new Vector3(125, 4.95f, 0), "east-lever");
            Platform(root.transform, "Core approach ledge", new Vector3(157, 1.35f, 0), 4);
            Platform(root.transform, "Core overlook", new Vector3(162, 2.9f, 0), 4);
            Platform(root.transform, "Arena cover ledge", new Vector3(181, 1.3f, 0), 3);
            Platform(root.transform, "Arena far ledge", new Vector3(199, 1.3f, 0), 3);
            foreach (float position in new[] { 28f, 56f, 72f, 110f, 131f, 143f, 166f }) Press(root.transform, position, position % 4);
            foreach (float position in new[] { 31f, 60f, 99f, 154f }) Crate(root.transform, position);
            int enemyNumber = 0;
            foreach (float position in new[] { 10f, 26f, 39f, 51f, 62f, 70f, 84f, 106f, 116f, 127f, 140f, 154f, 164f })
                SpawnEnemy(root.transform, EnemyKind.Patrol, new Vector3(position, 0, 0), "patrol-" + enemyNumber++, 3);
            foreach (float position in new[] { 33f, 59f, 89f, 112f, 145f, 168f })
                SpawnEnemy(root.transform, EnemyKind.Turret, new Vector3(position, 0, 0), "turret-" + enemyNumber++, 4);
            foreach (float position in new[] { 52f, 80f, 118f, 135f, 160f })
                SpawnEnemy(root.transform, EnemyKind.Drone, new Vector3(position, 2.1f, 0), "drone-" + enemyNumber++, 3);
            SpawnEnemy(root.transform, EnemyKind.Warden, new Vector3(191, 0, 0), "warden", 30);
            if (!progress.doubleJump) Interactable(root.transform, WorldInteractable.Kind.DoubleJump, new Vector3(103, 0, 0), "double-jump");
            Vector3[] pickups = { new Vector3(16.5f, 2.4f, 0), new Vector3(40, 3.8f, 0), new Vector3(54, 0.8f, 0), new Vector3(87, 3.4f, 0), new Vector3(91, 4.7f, 0), new Vector3(115, 3.9f, 0), new Vector3(128, 3.7f, 0), new Vector3(162, 3.3f, 0), new Vector3(181, 1.7f, 0), new Vector3(199, 1.7f, 0) };
            for (int index = 0; index < pickups.Length; index++)
                if (!progress.collected.Contains("battery-" + index)) Interactable(root.transform, WorldInteractable.Kind.Battery, pickups[index], "battery-" + index);
            Interactable(root.transform, WorldInteractable.Kind.Terminal, new Vector3(207, 0, 0), "delivery");
            Tutorial(root.transform, 5, "能源芯的最后一程。A / D 移动，空格短按小跳，长按高跳。");
            Tutorial(root.transform, 9, "J 挥刃。敌人红灯蓄力时，面向攻击方向按 K 精准格挡。");
            Tutorial(root.transform, 17, "Shift 冲刺穿过缺口。空中仅能冲刺一次，落地后恢复。");
            Tutorial(root.transform, 27, "压机红灯提前预警，等它升起再通过。被压会损失两格生命。");
            Tutorial(root.transform, 65, "升降井。E 激活中继站保存位置；死亡后恢复上次中继站。");
            Tutorial(root.transform, 101, "拾取双级擒纵器：在前方装置旁按 E，获得永久二段跳。");
            Tutorial(root.transform, 107, "二段跳可抵达高处回路。两条高架支路的拉杆能永久打开捷径。");
            Tutorial(root.transform, 134, "钟面核心。远程弹丸可精准格挡反击；普通无敌时间不会反弹。");
            Tutorial(root.transform, 178, "守门体：跳过低位冲击波；散射蓄力时靠近进攻。半血后进入过载阶段。");
            BuildBackdrop(root.transform);
            BuildHero(root.transform);
            return root;
        }

        public static GameObject Solid(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.layer = 8;
            ArtFactory.Part(root.transform, name + " bevelled surface", Vector3.zero, size, material, 0.035f);
            root.AddComponent<BoxCollider>().size = size;
            return root;
        }

        private static void Ground(Transform parent, float center, float width)
        {
            Solid(parent, "Structural floor", new Vector3(center, -0.65f, 0), new Vector3(width, 1.3f, 3.2f), ArtFactory.Steel);
            ArtFactory.Part(parent, "Brass floor trim", new Vector3(center, -0.09f, -1.64f), new Vector3(width, 0.13f, 0.12f), ArtFactory.Brass);
            for (float offset = -width / 2 + 0.5f; offset < width / 2; offset += 1.2f)
            {
                ArtFactory.Part(parent, "Deck panel seam", new Vector3(center + offset, 0.008f, 0), new Vector3(0.03f, 0.015f, 2.8f), ArtFactory.Dark);
                ArtFactory.Part(parent, "Deck rivet", new Vector3(center + offset, -0.35f, -1.67f), new Vector3(0.07f, 0.07f, 0.035f), ArtFactory.Brass);
            }
        }

        private static GameObject Platform(Transform parent, string name, Vector3 position, float width)
        {
            var platform = Solid(parent, name, position, new Vector3(width, 0.4f, 2.2f), ArtFactory.Steel);
            ArtFactory.Part(platform.transform, "Step edge", new Vector3(0, 0.17f, -1.13f), new Vector3(width, 0.08f, 0.07f), ArtFactory.Brass);
            return platform;
        }

        public static void ShortcutBridge(Transform parent, float position)
        {
            Platform(parent, "Permanent shortcut bridge", new Vector3(position, -0.2f, 0), 4.5f);
        }

        private static void Checkpoint(Transform parent, int identifier, float position)
        {
            var station = Interactable(parent, WorldInteractable.Kind.Checkpoint, new Vector3(position, 0, 0), "checkpoint-" + identifier);
            station.checkpoint = identifier;
        }

        private static WorldInteractable Interactable(Transform parent, WorldInteractable.Kind kind, Vector3 position, string identifier)
        {
            var root = ArtFactory.Pivot(parent, kind + " " + identifier, position);
            var interaction = root.gameObject.AddComponent<WorldInteractable>();
            interaction.kind = kind;
            interaction.identifier = identifier;
            if (kind == WorldInteractable.Kind.Battery)
            {
                var cell = ArtFactory.Pivot(root, "Rotating battery", new Vector3(0, 0.5f, 0));
                ArtFactory.Part(cell, "Battery shell", Vector3.zero, new Vector3(0.33f, 0.48f, 0.33f), ArtFactory.Brass);
                ArtFactory.Part(cell, "Charge gauge", new Vector3(0, 0, -0.18f), new Vector3(0.17f, 0.33f, 0.03f), ArtFactory.Teal);
            }
            else
            {
                bool terminal = kind == WorldInteractable.Kind.Terminal;
                float height = terminal ? 2.6f : 1.5f;
                ArtFactory.Part(root, "Cast pedestal", new Vector3(0, 0.18f, 0.8f), new Vector3(1.3f, 0.36f, 0.8f), ArtFactory.Steel);
                ArtFactory.Part(root, "Enamel console", new Vector3(0, height / 2 + 0.25f, 0.8f), new Vector3(0.7f, height, 0.45f), ArtFactory.Enamel);
                ArtFactory.Ring(root, "Relay ring", new Vector3(0, height - 0.1f, 0.5f), terminal ? 0.5f : 0.32f, terminal ? 0.38f : 0.24f, 0.12f, ArtFactory.Brass, 16);
                ArtFactory.Ring(root, "Relay core", new Vector3(0, height - 0.1f, 0.42f), terminal ? 0.3f : 0.19f, 0, 0.05f, ArtFactory.Teal);
                ArtFactory.Part(root, "Console readout", new Vector3(0, 0.65f, 0.55f), new Vector3(0.42f, 0.16f, 0.06f), ArtFactory.Dark);
            }
            return interaction;
        }

        private static void Tutorial(Transform parent, float position, string message)
        {
            var root = ArtFactory.Pivot(parent, "Guidance beacon", new Vector3(position, 0, 0));
            var interaction = root.gameObject.AddComponent<WorldInteractable>();
            interaction.kind = WorldInteractable.Kind.Tutorial;
            interaction.message = message;
        }

        private static void Press(Transform parent, float position, float phase)
        {
            var root = ArtFactory.Pivot(parent, "Steam press", new Vector3(position, 0, 0));
            ArtFactory.Part(root, "Press gantry", new Vector3(0, 5.4f, 0.8f), new Vector3(2.7f, 0.5f, 1.6f), ArtFactory.Steel);
            ArtFactory.Part(root, "Press support", new Vector3(1.2f, 2.7f, 1.4f), new Vector3(0.3f, 5.4f, 0.3f), ArtFactory.Steel);
            var piston = ArtFactory.Pivot(root, "Press head", new Vector3(0, 3.8f, 0));
            ArtFactory.Part(piston, "Press platen", Vector3.zero, new Vector3(1.8f, 0.8f, 1.7f), ArtFactory.Brass);
            ArtFactory.Part(piston, "Hydraulic ram", new Vector3(0, 2, 0.5f), new Vector3(0.3f, 4, 0.3f), ArtFactory.Enamel);
            var lamp = ArtFactory.Part(root, "Danger lamp", new Vector3(0, 4.9f, -0.2f), new Vector3(0.5f, 0.15f, 0.15f), ArtFactory.Red);
            var hazard = root.gameObject.AddComponent<PressHazard>();
            hazard.offset = phase;
            hazard.piston = piston;
            hazard.lamp = lamp.transform;
        }

        private static void Crate(Transform parent, float position)
        {
            var crate = Solid(parent, "Breakable crate", new Vector3(position, 0.65f, 0), new Vector3(1.0f, 1.3f, 1.2f), ArtFactory.Brass);
            crate.AddComponent<Breakable>();
            ArtFactory.Part(crate.transform, "Crate diagonal", new Vector3(0, 0, -0.62f), new Vector3(0.12f, 1.35f, 0.05f), ArtFactory.Dark).transform.localRotation = Quaternion.Euler(0, 0, 36);
        }

        public static EnemyActor SpawnEnemy(Transform parent, EnemyKind kind, Vector3 position, string identifier, int hitpoints)
        {
            var root = ArtFactory.Pivot(parent, kind + " " + identifier, position);
            var actor = root.gameObject.AddComponent<EnemyActor>();
            actor.kind = kind;
            actor.identifier = identifier;
            actor.health = actor.maximumHealth = hitpoints;
            var collider = root.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            if (kind == EnemyKind.Patrol || kind == EnemyKind.Warden)
            {
                float scale = kind == EnemyKind.Warden ? 1.9f : 1;
                ArtFactory.Courier(root, true, scale);
                collider.size = new Vector3(0.8f, 1.8f, 0.7f) * scale;
                collider.center = Vector3.up * 0.9f * scale;
                if (kind == EnemyKind.Warden)
                {
                    ArtFactory.Ring(root, "Warden halo", new Vector3(0, 2, 0.55f), 1.6f, 1.4f, 0.18f, ArtFactory.Brass, 24).AddComponent<GearMotion>().speed = -8;
                }
            }
            else if (kind == EnemyKind.Turret)
            {
                ArtFactory.Part(root, "Turret pedestal", new Vector3(0, 0.3f, 0), new Vector3(0.9f, 0.6f, 0.8f), ArtFactory.Steel);
                ArtFactory.Ring(root, "Turret axle", new Vector3(0, 0.8f, 0), 0.37f, 0.21f, 0.5f, ArtFactory.Brass, 16);
                ArtFactory.Part(root, "Turret shroud", new Vector3(0, 0.95f, 0), new Vector3(0.8f, 0.45f, 0.6f), ArtFactory.Enamel);
                ArtFactory.Part(root, "Turret barrel", new Vector3(-0.6f, 0.85f, 0), new Vector3(0.8f, 0.16f, 0.16f), ArtFactory.Steel);
                ArtFactory.Part(root, "Tracking lens", new Vector3(0, 0.95f, -0.32f), new Vector3(0.32f, 0.08f, 0.03f), ArtFactory.Red);
                collider.size = new Vector3(1, 1.3f, 0.7f);
                collider.center = Vector3.up * 0.65f;
            }
            else
            {
                ArtFactory.Ring(root, "Drone stabilizer", Vector3.zero, 0.64f, 0.5f, 0.15f, ArtFactory.Brass, 16).AddComponent<GearMotion>().speed = 95;
                ArtFactory.Part(root, "Drone enamel core", Vector3.zero, new Vector3(0.64f, 0.5f, 0.4f), ArtFactory.Enamel);
                ArtFactory.Ring(root, "Drone optic", new Vector3(0, 0, -0.25f), 0.17f, 0, 0.05f, ArtFactory.Red);
                for (int side = -1; side <= 1; side += 2)
                    ArtFactory.Part(root, "Drone wing", new Vector3(side * 0.65f, 0.05f, 0), new Vector3(0.5f, 0.09f, 0.3f), ArtFactory.Steel);
                collider.size = new Vector3(1.2f, 0.9f, 0.7f);
            }
            var indicator = ArtFactory.Ring(root, "Attack indicator", new Vector3(0, kind == EnemyKind.Warden ? 4.2f : kind == EnemyKind.Drone ? 0.9f : 2.3f, -0.1f), 0.18f, 0.105f, 0.05f, ArtFactory.Red);
            indicator.SetActive(false);
            return actor;
        }

        private static void BuildHero(Transform parent)
        {
            var root = ArtFactory.Pivot(parent, "Courier", new Vector3(3, 0.1f, 0));
            ArtFactory.Courier(root);
            var motor = root.gameObject.AddComponent<CharacterController>();
            motor.height = 1.8f;
            motor.radius = 0.28f;
            motor.center = Vector3.up * 0.9f;
            motor.stepOffset = 0.25f;
            motor.slopeLimit = 45;
            motor.skinWidth = 0.025f;
            root.gameObject.AddComponent<CourierController>();
        }

        private static void BuildBackdrop(Transform parent)
        {
            var masonry = ArtFactory.Material("Deep petrol masonry", new Color(0.055f, 0.105f, 0.125f), 0.12f, 0.26f);
            var lightGlass = ArtFactory.Material("Amber window glass", new Color(0.65f, 0.41f, 0.18f), 0.15f, 0.6f, 0.65f);
            var shaftGlass = ArtFactory.Material("Shaft blue glass", new Color(0.09f, 0.25f, 0.29f), 0.1f, 0.5f, 0.5f);
            var coreGlass = ArtFactory.Material("Core midnight glass", new Color(0.07f, 0.11f, 0.16f), 0.1f, 0.4f, 0.2f);
            var agedBronze = ArtFactory.Material("Aged background bronze", new Color(0.24f, 0.18f, 0.105f), 0.55f, 0.27f);
            for (int bay = 0; bay < 28; bay++)
            {
                float position = bay * 8;
                ArtFactory.Part(parent, "Factory wall bay", new Vector3(position, 4, 6.5f), new Vector3(7.9f, 15, 0.65f), masonry, 0.02f);
                ArtFactory.Part(parent, "Load bearing column", new Vector3(position - 3.8f, 4, 4), new Vector3(0.45f, 14, 0.6f), ArtFactory.Steel);
                for (int row = 0; row < 3; row++)
                {
                    ArtFactory.Part(parent, "Tall glazing", new Vector3(position, 2 + row * 3.5f, 6.05f), new Vector3(4.4f, 2.8f, 0.12f), position < 64 ? lightGlass : position < 133 ? shaftGlass : coreGlass);
                    for (int mullion = -2; mullion <= 2; mullion++)
                        ArtFactory.Part(parent, "Window mullion", new Vector3(position + mullion * 0.87f, 2 + row * 3.5f, 5.94f), new Vector3(0.065f, 2.9f, 0.1f), ArtFactory.Steel);
                    ArtFactory.Part(parent, "Window transom", new Vector3(position, 2 + row * 3.5f, 5.9f), new Vector3(4.5f, 0.08f, 0.1f), ArtFactory.Steel);
                }
                var gear = ArtFactory.Ring(parent, "Industrial gear", new Vector3(position + 1.5f, 2.5f + bay % 3, 3.9f), 1.7f + bay % 2 * 0.45f, 1.2f, 0.22f, agedBronze, 28);
                gear.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                gear.AddComponent<GearMotion>().speed = bay % 2 == 0 ? 8 : -11;
                for (int spoke = 0; spoke < 6; spoke++)
                {
                    var beam = ArtFactory.Part(gear.transform, "Gear spoke", Vector3.zero, new Vector3(0.12f, 3.0f, 0.14f), ArtFactory.Steel);
                    beam.transform.localRotation = Quaternion.Euler(0, 0, spoke * 30);
                }
                ArtFactory.Ring(gear.transform, "Bearing hub", Vector3.zero, 0.32f, 0.17f, 0.3f, ArtFactory.Enamel);
                ArtFactory.Part(parent, "Upper service pipe", new Vector3(position, 8.8f, 2.7f), new Vector3(8, 0.22f, 0.22f), ArtFactory.Brass);
                ArtFactory.Ring(parent, "Pipe flange", new Vector3(position - 3.5f, 8.8f, 2.7f), 0.22f, 0.12f, 0.2f, ArtFactory.Steel).transform.localRotation = Quaternion.Euler(0, 90, 0);
                if (bay % 3 == 0)
                {
                    var lamp = new GameObject("Warm industrial lamp");
                    lamp.transform.SetParent(parent, false);
                    lamp.transform.position = new Vector3(position, 5, -0.6f);
                    var light = lamp.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(1, 0.63f, 0.3f);
                    light.range = 10;
                    light.intensity = 2;
                    light.shadows = LightShadows.None;
                    ArtFactory.Part(parent, "Lamp housing", new Vector3(position, 5.2f, -0.6f), new Vector3(0.7f, 0.15f, 0.5f), ArtFactory.Brass);
                }
            }
            var clock = ArtFactory.Pivot(parent, "Astronomical clock face", new Vector3(191, 4.2f, 3.1f));
            ArtFactory.Ring(clock, "Clock rim", Vector3.zero, 5.0f, 4.65f, 0.3f, ArtFactory.Brass, 60);
            ArtFactory.Ring(clock, "Inner dial", Vector3.back * 0.1f, 4.4f, 4.32f, 0.08f, ArtFactory.Enamel);
            for (int mark = 0; mark < 12; mark++)
            {
                float angle = mark * Mathf.PI / 6;
                var tick = ArtFactory.Part(clock, "Clock hour", new Vector3(Mathf.Sin(angle) * 4, Mathf.Cos(angle) * 4, -0.2f), new Vector3(0.12f, 0.5f, 0.09f), ArtFactory.Enamel);
                tick.transform.localRotation = Quaternion.Euler(0, 0, -mark * 30);
            }
            var hand = ArtFactory.Pivot(clock, "Minute axle", Vector3.back * 0.3f);
            ArtFactory.Part(hand, "Clock hand", new Vector3(0, 1.6f, 0), new Vector3(0.15f, 3.4f, 0.1f), ArtFactory.Teal);
            hand.gameObject.AddComponent<GearMotion>().speed = -3;
        }

        public static void EnsurePresentation()
        {
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5.9f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 65;
                camera.backgroundColor = new Color(0.025f, 0.055f, 0.07f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                cameraObject.AddComponent<AudioListener>();
                cameraObject.transform.position = new Vector3(8, 3.5f, -17);
                cameraObject.transform.rotation = Quaternion.Euler(8, 0, 0);
            }
            if (GameObject.Find("Key light") == null)
            {
                var key = new GameObject("Key light");
                key.transform.rotation = Quaternion.Euler(38, -32, 0);
                var light = key.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(0.72f, 0.85f, 1);
                light.intensity = 1.65f;
                light.shadows = LightShadows.Soft;
                var fill = new GameObject("Warm rim");
                fill.transform.rotation = Quaternion.Euler(25, 150, 0);
                var rim = fill.AddComponent<Light>();
                rim.type = LightType.Directional;
                rim.color = new Color(1, 0.69f, 0.36f);
                rim.intensity = 0.75f;
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.25f, 0.36f, 0.4f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.2f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.09f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.075f, 0.14f, 0.16f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 22;
            RenderSettings.fogEndDistance = 52;
        }
    }
}
