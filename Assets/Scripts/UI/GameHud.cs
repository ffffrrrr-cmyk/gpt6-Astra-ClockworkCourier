using System;
using UnityEngine;

namespace ClockworkCourier
{
    public sealed class GameHud : MonoBehaviour
    {
        public static bool ModalOpen { get; private set; }
        private GUIStyle label;
        private GUIStyle heading;
        private GUIStyle caption;
        private GUIStyle button;
        private GUIStyle body;
        private Font font;
        private bool confirmNew;
        private bool showHelp;
        private bool showSettings;
        private readonly Color ink = new Color(0.045f, 0.085f, 0.10f, 0.96f);
        private readonly Color cream = new Color(0.91f, 0.88f, 0.76f);
        private readonly Color teal = new Color(0.28f, 0.91f, 0.80f);
        private readonly Color brass = new Color(0.82f, 0.59f, 0.31f);

        private void Initialize()
        {
            if (font != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Arial" }, 22);
            label = new GUIStyle(GUI.skin.label) { font = font, fontSize = 21, normal = { textColor = cream } };
            heading = new GUIStyle(label) { fontSize = 66, fontStyle = FontStyle.Bold };
            caption = new GUIStyle(label) { fontSize = 13, normal = { textColor = brass } };
            body = new GUIStyle(label) { fontSize = 19, wordWrap = true, normal = { textColor = new Color(0.70f, 0.77f, 0.74f) } };
            button = new GUIStyle(label) { fontSize = 22, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(22, 12, 0, 0) };
        }

        private void OnGUI()
        {
            Initialize();
            ModalOpen = showHelp || showSettings || confirmNew;
            var session = GameSession.Instance;
            if (session == null || session.Progress == null) return;
            float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            float offsetX = (Screen.width - 1440 * scale) / 2;
            float offsetY = (Screen.height - 900 * scale) / 2;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            GUI.enabled = !ModalOpen;
            if (session.Phase == RunPhase.Menu) DrawMenu(session);
            else
            {
                DrawGameplay(session);
                if (session.Phase == RunPhase.Paused) DrawPause(session);
                if (session.Phase == RunPhase.Dead) DrawDead(session);
                if (session.Phase == RunPhase.Complete) DrawComplete(session);
            }
            GUI.enabled = true;
            if (showHelp) DrawHelp();
            if (showSettings) DrawSettings(session);
            if (confirmNew) DrawConfirmation(session);
        }

        private void Panel(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void Text(float left, float top, float width, float height, string value, GUIStyle style = null)
        {
            GUI.Label(new Rect(left, top, width, height), value, style ?? label);
        }

        private bool Button(float left, float top, float width, string text, bool primary = false)
        {
            Rect rect = new Rect(left, top, width, 55);
            bool hover = rect.Contains(Event.current.mousePosition);
            Panel(rect, primary ? teal : hover ? new Color(0.19f, 0.28f, 0.28f, 1) : new Color(0.10f, 0.17f, 0.18f, 1));
            Panel(new Rect(left, top, 3, 55), primary ? cream : brass);
            button.normal.textColor = primary ? ink : cream;
            return GUI.Button(rect, text, button);
        }

        private void DrawMenu(GameSession session)
        {
            Panel(new Rect(-200, -100, 820, 1100), ink);
            Panel(new Rect(620, 0, 1, 900), brass * 0.7f);
            Text(66, 62, 490, 25, "CLOCKWORK COURIER     /     U2", caption);
            Text(60, 132, 540, 93, "发条信使", heading);
            Text(67, 240, 480, 32, "在停摆之前，送达最后一枚能源芯。");
            Text(67, 299, 462, 78, "穿过齿轮工坊与升降井，学会在机械的节拍之间跳跃、格挡与反击。钟面核心仍在等待。", body);
            Text(67, 395, 460, 24, "三段旅程  /  永久能力  /  精准格挡", caption);
            if (Button(67, 446, 445, session.Progress.elapsed > 0 ? "继续旅程    →" : "开始旅程    →", true)) session.Begin(false);
            if (Button(67, 514, 445, "操作与旅程指南")) showHelp = true;
            if (Button(67, 582, 214, "设置")) showSettings = true;
            if (Button(298, 582, 214, "重新开始")) confirmNew = true;
            if (session.Progress.completed)
            {
                Text(67, 666, 470, 24, "已解锁区域计时挑战", caption);
                if (Button(67, 705, 140, "工坊")) session.Begin(false, 0);
                if (Button(219, 705, 140, "升降井")) session.Begin(false, 2);
                if (Button(371, 705, 140, "核心")) session.Begin(false, 4);
            }
            else Text(67, 684, 440, 60, "存档：中继站 " + (session.Progress.checkpoint + 1) + " / 6\n二段跳：" + (session.Progress.doubleJump ? "已安装" : "等待安装"), body);
            Text(67, 828, 480, 32, "单人  ·  键盘操作  ·  本地存档", caption);
            Text(1030, 789, 360, 35, "01 — THE LAST DELIVERY", caption);
            Text(1030, 822, 350, 42, "齿轮工坊", new GUIStyle(label) { fontSize = 30 });
            if (Button(1200, 50, 185, "退出游戏")) Application.Quit();
            if (SaveStore.LastWarning != null) Text(670, 70, 690, 65, SaveStore.LastWarning, body);
        }

        private void DrawGameplay(GameSession session)
        {
            if (session.Hero == null) return;
            Panel(new Rect(34, 28, 375, 112), ink);
            Text(52, 37, 340, 24, "COURIER  /  能源芯载体", caption);
            for (int index = 0; index < VitalState.MaximumHealth; index++)
                Panel(new Rect(54 + index * 43, 75, 31, 12), index < session.Hero.Vitals.Health ? cream : new Color(0.24f, 0.28f, 0.28f));
            Panel(new Rect(54, 104, 277, 5), new Color(0.2f, 0.29f, 0.29f));
            Panel(new Rect(54, 104, 277 * session.Hero.Vitals.Energy / 100, 5), teal);
            Text(342, 91, 60, 32, Mathf.RoundToInt(session.Hero.Vitals.Energy).ToString(), caption);
            Panel(new Rect(980, 28, 425, 112), ink);
            Text(1000, 39, 390, 35, Campaign.Regions[Campaign.Region(session.Hero.transform.position.x)]);
            Text(1000, 85, 390, 28, "回收 " + session.Progress.salvage.ToString("00") + "   /   中继 " + (session.Progress.checkpoint + 1) + "   /   " + TimeString(session.Challenge ? session.RunClock : session.Progress.elapsed), caption);
            float route = Mathf.Clamp01(session.Hero.transform.position.x / 207);
            Panel(new Rect(34, 160, 1370, 2), new Color(0.4f, 0.55f, 0.53f, 0.3f));
            Panel(new Rect(34, 160, 1370 * route, 2), teal);
            Text(37, 176, 900, 30, session.Progress.doubleJump ? "双级擒纵器已安装  /  空中再按空格二段跳" : "任务  /  找到升降井的双级擒纵器，再抵达钟面核心", caption);
            if (session.Hero.transform.position.x > 177 && !session.WardenDefeated)
            {
                var actors = FindObjectsByType<EnemyActor>();
                foreach (var actor in actors)
                    if (actor.kind == EnemyKind.Warden && !actor.Dead)
                    {
                        Panel(new Rect(500, 224, 440, 52), ink);
                        Text(516, 229, 420, 30, actor.health <= actor.maximumHealth / 2 ? "守门机械体  /  过载阶段" : "守门机械体  /  封锁阶段", caption);
                        Panel(new Rect(516, 261, 408 * actor.health / actor.maximumHealth, 5), brass);
                    }
            }
            foreach (var interaction in FindObjectsByType<WorldInteractable>())
            {
                if (!interaction.InRange || string.IsNullOrEmpty(interaction.Prompt)) continue;
                Panel(new Rect(420, 695, 600, 49), ink);
                Text(442, 704, 565, 37, interaction.Prompt);
                break;
            }
            if (Time.unscaledTime < session.ToastUntil)
            {
                Panel(new Rect(255, 765, 930, 55), ink);
                Text(276, 776, 890, 42, session.Toast, body);
            }
            Panel(new Rect(34, 844, 1370, 34), ink);
            Text(51, 849, 1350, 28, "A D 移动     SPACE 跳跃     SHIFT 冲刺     J 挥刃     K 格挡     E 交互                                         ESC 暂停", caption);
        }

        private void Modal(string eyebrow, string title)
        {
            Panel(new Rect(-200, -100, 1840, 1100), new Color(0.015f, 0.03f, 0.035f, 0.72f));
            Panel(new Rect(420, 195, 600, 520), ink);
            Panel(new Rect(420, 195, 600, 3), brass);
            Text(460, 230, 510, 25, eyebrow, caption);
            Text(457, 273, 530, 65, title, new GUIStyle(heading) { fontSize = 40 });
        }

        private void DrawPause(GameSession session)
        {
            Modal("PAUSED  /  机械暂歇", "旅程已暂停");
            if (Button(460, 370, 520, "返回旅程", true)) session.Resume();
            if (Button(460, 438, 250, "操作指南")) showHelp = true;
            if (Button(730, 438, 250, "设置")) showSettings = true;
            if (Button(460, 506, 520, "从中继站重试")) session.Retry();
            if (Button(460, 574, 520, "保存并返回主菜单")) session.Menu();
        }

        private void DrawDead(GameSession session)
        {
            Modal("SIGNAL LOST  /  信号中断", "能源芯仍然安全");
            Text(460, 360, 510, 90, "从最近的中继站恢复。已获得的永久能力、捷径与回收记录都会保留。", body);
            if (Button(460, 482, 520, "从中继站 " + (session.Progress.checkpoint + 1) + " 重试", true)) session.Retry();
            if (Button(460, 555, 520, "返回主菜单")) session.Menu();
        }

        private void DrawComplete(GameSession session)
        {
            Modal("DELIVERY COMPLETE  /  送达", "钟楼重新开始计时");
            Text(460, 359, 520, 110, "旅程用时  " + TimeString(session.Challenge ? session.RunClock : session.Progress.elapsed) + "\n回收零件  " + session.Progress.salvage + "       本次重启  " + session.RunDeaths + "\n区域选择与计时挑战已开放。", body);
            if (Button(460, 509, 520, "返回主菜单", true)) session.Menu();
        }

        private void DrawHelp()
        {
            Panel(new Rect(-200, -100, 1840, 1100), new Color(0.015f, 0.03f, 0.035f, 0.9f));
            Panel(new Rect(270, 95, 900, 710), ink);
            Text(310, 126, 810, 28, "FIELD MANUAL  /  信使手册", caption);
            Text(305, 174, 820, 65, "把握机械之间的空隙", new GUIStyle(heading) { fontSize = 39 });
            string[] lines = { "A / D 或左右方向键：移动。空格短按小跳，长按高跳。", "Shift：冲刺。空中一次，落地恢复；低电量时仍能基础冲刺。", "J：挥刃，注意攻击前摇与恢复；一次挥击不会重复伤害同一敌人。", "K：面向敌人在攻击到达前格挡。成功回流电量，并反击弹丸。", "E：中继站保存并恢复；装置安装能力；拉杆开启永久捷径。", "升降井中获得二段跳后，可回到工坊高架与升降井高架探索。", "敌人红色环亮起表示蓄力。压机红灯后下压，留意节奏。", "守门体的低位波可跳过，散射可近身或格挡。半血后加速。", "Esc：暂停。存档位于当前用户的 LocalLow/ClockworkCourier/U2。" };
            for (int index = 0; index < lines.Length; index++) Text(310, 272 + index * 43, 810, 42, lines[index], body);
            if (Button(310, 711, 810, "明白了，返回", true)) showHelp = false;
        }

        private void DrawSettings(GameSession session)
        {
            Modal("OPTIONS  /  系统校准", "声音与显示");
            Text(460, 368, 220, 35, "主音量  " + Mathf.RoundToInt(session.Volume * 100) + "%");
            session.Volume = GUI.HorizontalSlider(new Rect(700, 382, 270, 24), session.Volume, 0, 1);
            if (Button(460, 430, 520, "震动与强反馈：" + (session.ScreenShake ? "开启" : "降低"))) session.ScreenShake = !session.ScreenShake;
            if (Button(460, 498, 520, "阴影质量：" + (QualitySettings.shadows == ShadowQuality.Disable ? "低" : "高")))
            {
                bool enable = QualitySettings.shadows == ShadowQuality.Disable;
                QualitySettings.shadows = enable ? ShadowQuality.All : ShadowQuality.Disable;
                foreach (var light in FindObjectsByType<Light>())
                    if (light.type == LightType.Directional && light.name == "Key light") light.shadows = enable ? LightShadows.Soft : LightShadows.None;
            }
            if (Button(460, 579, 520, "保存设置并返回", true))
            {
                PlayerPrefs.SetFloat("U2.Volume", session.Volume);
                PlayerPrefs.SetInt("U2.Shake", session.ScreenShake ? 1 : 0);
                PlayerPrefs.Save();
                showSettings = false;
            }
        }

        private void DrawConfirmation(GameSession session)
        {
            Modal("NEW JOURNEY  /  重新出发", "重置旅程进度？");
            Text(460, 360, 515, 90, "将重置当前中继站、永久能力、捷径和回收记录。此操作会替换本地存档。", body);
            if (Button(460, 490, 520, "确认，开始新旅程", true)) { confirmNew = false; session.Begin(true); }
            if (Button(460, 562, 520, "取消")) confirmNew = false;
        }

        private static string TimeString(float value)
        {
            var time = TimeSpan.FromSeconds(value);
            return ((int)time.TotalMinutes).ToString("00") + ":" + time.Seconds.ToString("00");
        }
    }
}
