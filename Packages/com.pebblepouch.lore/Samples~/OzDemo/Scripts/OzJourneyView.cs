using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using S = PebblePouch.Lore.Samples.OzDemo.OzJourneyStyles;

namespace PebblePouch.Lore.Samples.OzDemo
{
    // Draws the Oz Demo with IMGUI (no UI packages needed). Everything shown is read from Lore at the moment it is
    // drawn, so runtime changes (LoreState) appear immediately; values that differ from Lore get an orange dot.
    //
    // Left: the map (Places at their Map Position, in their Color). Middle: a card of the selected Entry with its
    // Fields, description links and actions. Right: Quests, inventory and the journal. All rules are in OzJourney.
    [RequireComponent(typeof(OzJourney))]
    public sealed class OzJourneyView : MonoBehaviour
    {
        #region Fields
        // Drawn at this virtual size, scaled to fit the screen.
        private const float ScreenWidth = 1280f;
        private const float ScreenHeight = 720f;

        private const float Gap = 10f;
        private const float TopBarHeight = 56f;
        private const float MapWidth = 470f;
        private const float CardWidth = 440f;
        private const float MapPadding = 36f;
        private const float PlaceSize = 24f;
        private const float MarkerSize = 34f;
        private const float RowLabelWidth = 100f;
        private const float PortraitSize = 72f;
        private const float IconSize = 40f;
        private const float DotSize = 9f;

        private OzJourney _journey;
        private Entry _selected;
        private Entry _talkingTo;
        private int _dialogueLine;
        private Vector2 _cardScroll;
        private Vector2 _journalScroll;

        // Actions run after drawing, so a click never changes what the rest of the same OnGUI event draws.
        private Action _pending;
        #endregion

        #region Unity
        private void Awake()
        {
            _journey = GetComponent<OzJourney>();

            AddRenderPipelineCameraData();
        }

        private void OnGUI()
        {
            if (!_journey.enabled)
                return;

            S.Ensure();

            float scale = Mathf.Min(Screen.width / ScreenWidth, Screen.height / ScreenHeight);

            GUI.matrix = Matrix4x4.TRS(
                new Vector3((Screen.width - ScreenWidth * scale) / 2f, (Screen.height - ScreenHeight * scale) / 2f),
                Quaternion.identity,
                new Vector3(scale, scale, 1f));

            _selected ??= _journey.CurrentPlace;

            float contentTop = TopBarHeight + Gap;
            float contentHeight = ScreenHeight - contentTop - Gap;
            float sideWidth = ScreenWidth - MapWidth - CardWidth - 4f * Gap;

            DrawTopBar(new Rect(Gap, Gap, ScreenWidth - 2f * Gap, TopBarHeight - Gap));
            DrawMap(new Rect(Gap, contentTop, MapWidth, contentHeight));
            DrawCard(new Rect(2f * Gap + MapWidth, contentTop, CardWidth, contentHeight));
            DrawSide(new Rect(3f * Gap + MapWidth + CardWidth, contentTop, sideWidth, contentHeight));
            DrawTooltip();

            if (_pending != null && Event.current.type != EventType.Layout)
            {
                Action action = _pending;

                _pending = null;
                action();
            }
        }
        #endregion

        #region Top bar
        private void DrawTopBar(Rect rect)
        {
            Entry player = _journey.Player;

            S.Fill(rect, S.Panel);

            var portrait = new Rect(rect.x + 6f, rect.y + 4f, rect.height - 8f, rect.height - 8f);

            S.DrawSprite(portrait, player.GetSprite(OzFields.Portrait));

            if (GUI.Button(portrait, GUIContent.none, GUIStyle.none))
                Select(player);

            GUI.Label(new Rect(portrait.xMax + 8f, rect.y + 2f, 300f, 26f), _journey.Lore.Title, S.Title);
            GUI.Label(new Rect(portrait.xMax + 8f, rect.y + 26f, 300f, 18f), $"Playing {player.Title}", S.DimLabel);

            DrawHealth(new Rect(rect.x + 380f, rect.y + 14f, 220f, 18f), player);

            float x = rect.xMax - 6f;

            x = TopButton(x, rect, "New Game", true, () => Do(NewGame));
            x = TopButton(x, rect, "Load", _journey.HasSave, () => Do(_journey.Load));
            x = TopButton(x, rect, "Save", true, () => Do(_journey.Save));

            string state = LoreState.HasChanges ? "Runtime changes in LoreState" : "All values from Lore";
            var stateRect = new Rect(x - 230f, rect.y + 14f, 220f, 20f);

            S.DrawDisc(new Rect(stateRect.x, stateRect.y + 5f, DotSize, DotSize), LoreState.HasChanges ? S.Changed : S.Upcoming);
            GUI.Label(new Rect(stateRect.x + 14f, stateRect.y, stateRect.width, stateRect.height), state, S.DimLabel);
        }

        private static float TopButton(float x, Rect bar, string text, bool enabled, Action onClick)
        {
            const float width = 90f;

            x -= width;

            GUI.enabled = enabled;

            if (GUI.Button(new Rect(x, bar.y + 10f, width, 26f), text, S.Button))
                onClick();

            GUI.enabled = true;

            return x - 6f;
        }

        // Current Health against the value from Lore (GetOriginalInt), so damage stays visible.
        private static void DrawHealth(Rect rect, Entry character)
        {
            int health = character.GetInt(OzFields.Health);
            int original = Mathf.Max(1, character.GetOriginalInt(OzFields.Health));

            S.Fill(rect, S.HealthLost);
            S.Fill(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01((float)health / original), rect.height), S.HealthBar);

            GUI.Label(rect, $"  Health {health} / {original}", S.Small);
        }
        #endregion

        #region Map
        private void DrawMap(Rect rect)
        {
            S.Fill(rect, S.Panel);

            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 20f), "Map of Oz", S.Heading);

            var ground = new Rect(rect.x + 10f, rect.y + 32f, rect.width - 20f, rect.width - 20f);

            S.Fill(ground, S.MapGround);

            // Map Position is 0–100 on both axes, north (y = 100) at the top. Kansas has none: it lies beyond the
            // desert, in the corner of the map.
            Entry hovered = null;

            foreach (Entry place in _journey.Places)
            {
                Rect box = GetPlaceRect(ground, place, PlaceSize);

                if (box.Contains(Event.current.mousePosition))
                    hovered = place;
            }

            foreach (Entry place in _journey.Places)
                DrawPlace(ground, place, place == hovered);

            DrawPlayerMarker(ground);
            DrawLegend(new Rect(rect.x + 10f, ground.yMax + 8f, rect.width - 20f, rect.yMax - ground.yMax - 16f));
        }

        // Places are boxes, the player is a round portrait.
        private void DrawPlace(Rect ground, Entry place, bool hovered)
        {
            Rect box = GetPlaceRect(ground, place, PlaceSize);
            bool here = place == _journey.CurrentPlace;
            bool selected = place == _selected;

            if (selected || hovered)
                S.Fill(Grow(box, 4f), selected ? S.Accent : Color.white);

            S.Fill(Grow(box, 2f), Color.black);
            S.Fill(box, place.GetColor(OzFields.Color));

            if (place.GetBool(OzFields.Dangerous))
                S.DrawDisc(new Rect(box.xMax - 6f, box.y - 4f, 10f, 10f), S.Danger);

            if (GUI.Button(box, new GUIContent(string.Empty, place.Title), GUIStyle.none))
                Select(place);

            if (here || selected || hovered)
            {
                GUI.Label(
                    new Rect(box.center.x - 80f, box.yMax + 4f, 160f, 18f),
                    place.Title,
                    S.MapLabel);
            }
        }

        // Dorothy's portrait above her Location.
        private void DrawPlayerMarker(Rect ground)
        {
            Entry place = _journey.CurrentPlace;

            if (place == null)
                return;

            Rect box = GetPlaceRect(ground, place, PlaceSize);
            var marker = new Rect(box.center.x - MarkerSize / 2f, box.y - MarkerSize - 4f, MarkerSize, MarkerSize);

            S.DrawSprite(marker, _journey.Player.GetSprite(OzFields.Portrait));
        }

        // One swatch per Region, in the Color of its first Place.
        private void DrawLegend(Rect rect)
        {
            float x = rect.x;
            float y = rect.y;

            foreach (IGrouping<string, Entry> region in _journey.Places.GroupBy(place => place.GetEnum(OzFields.Region)))
            {
                float width = S.Small.CalcSize(new GUIContent(region.Key)).x + 22f;

                if (x + width > rect.xMax)
                {
                    x = rect.x;
                    y += 18f;
                }

                S.Fill(new Rect(x, y + 4f, 10f, 10f), region.First().GetColor(OzFields.Color));
                GUI.Label(new Rect(x + 14f, y, width, 18f), region.Key, S.Small);

                x += width + 6f;
            }

            y += 24f;

            S.DrawDisc(new Rect(rect.x, y + 4f, 10f, 10f), S.Danger);
            GUI.Label(new Rect(rect.x + 14f, y, rect.width, 18f), "Dangerous: costs Health to enter", S.Small);
        }

        private static Rect GetPlaceRect(Rect ground, Entry place, float size)
        {
            Vector2 position = place.GetVector2(OzFields.MapPosition);

            float x = Mathf.Lerp(ground.x + MapPadding, ground.xMax - MapPadding, position.x / 100f);
            float y = Mathf.Lerp(ground.yMax - MapPadding, ground.y + MapPadding, position.y / 100f);

            return new Rect(x - size / 2f, y - size / 2f, size, size);
        }

        private static Rect Grow(Rect rect, float amount) =>
            new(rect.x - amount, rect.y - amount, rect.width + 2f * amount, rect.height + 2f * amount);
        #endregion

        #region Card
        private void DrawCard(Rect rect)
        {
            S.Fill(rect, S.Panel);

            if (_selected == null)
                return;

            GUILayout.BeginArea(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, rect.height - 16f));

            _cardScroll = GUILayout.BeginScrollView(_cardScroll);

            DrawCardHeader(_selected);
            DrawDescription(_selected);

            switch (_selected.Category != null ? _selected.Category.Title : string.Empty)
            {
                case "Places":
                    DrawPlaceCard(_selected);
                    break;
                case "Characters":
                    DrawCharacterCard(_selected);
                    break;
                case "Items":
                    DrawItemCard(_selected);
                    break;
                case "Quests":
                    DrawQuestCard(_selected);
                    break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // Picture (Portrait, Icon or the Place's Banner), title, subtitle, Category and Tags in their colors.
        private void DrawCardHeader(Entry entry)
        {
            GUILayout.BeginHorizontal();

            Rect picture = GUILayoutUtility.GetRect(PortraitSize, PortraitSize, GUILayout.Width(PortraitSize));

            if (Event.current.type == EventType.Repaint)
                DrawPicture(picture, entry);

            GUILayout.BeginVertical();
            GUILayout.Label($"{entry.Glyph} {entry.Title}", S.Title);

            if (!string.IsNullOrEmpty(entry.Subtitle))
                GUILayout.Label(entry.Subtitle, S.DimLabel);

            GUILayout.BeginHorizontal();

            if (entry.Category != null)
                DrawChip(entry.Category.Title, entry.Category.Color);

            foreach (TagDefinition tag in _journey.Lore.GetTags(entry))
                DrawChip(tag.Title, tag.Color);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
        }

        private static void DrawPicture(Rect rect, Entry entry)
        {
            // HasField first: a getter for a Field the Entry does not have logs a warning.
            Sprite sprite =
                entry.HasField(OzFields.Portrait) ? entry.GetSprite(OzFields.Portrait) :
                entry.HasField(OzFields.Icon) ? entry.GetSprite(OzFields.Icon) :
                null;

            if (sprite != null)
            {
                S.DrawSprite(rect, sprite);

                return;
            }

            // Object Field: any Unity asset type, here a Texture2D.
            if (entry.HasField(OzFields.Banner))
            {
                var banner = entry.GetObject<Texture2D>(OzFields.Banner);

                if (banner != null)
                    GUI.DrawTexture(rect, banner, ScaleMode.ScaleToFit);

                return;
            }

            S.DrawDisc(rect, entry.Category != null ? entry.Category.Color : S.Upcoming);
        }

        private static void DrawChip(string text, Color color)
        {
            var content = new GUIContent(text);
            Rect rect = GUILayoutUtility.GetRect(content, S.Chip, GUILayout.ExpandWidth(false));

            S.Fill(rect, color);
            GUI.Label(rect, content, S.Chip);
        }

        // The description as rich text with links in color; below, one button per linked Entry. Links resolve
        // by ID, so they follow renames; a broken link has no Entry.
        private void DrawDescription(Entry entry)
        {
            if (string.IsNullOrEmpty(entry.Description))
                return;

            LoreProject lore = _journey.Lore;

            GUILayout.Label(
                lore.GetRichText(entry.Description, part => $"<color={S.LinkHex}>{part.Text}</color>"),
                S.Label);

            List<Entry> links = lore.ParseDescription(entry.Description)
                .Where(part => part.IsLink && part.Entry != null)
                .Select(part => part.Entry)
                .Distinct()
                .ToList();

            if (links.Count > 0)
                EntryLinks("Links", links);

            GUILayout.Space(8f);
        }

        private void DrawPlaceCard(Entry place)
        {
            Section("Place");
            Row(place, OzFields.Region, "Region", place.GetEnum(OzFields.Region));
            EntryRow(place, OzFields.Ruler, "Ruler");
            EntriesRow(place, OzFields.Inhabitants, "Inhabitants");
            Row(place, OzFields.Dangerous, "Dangerous", YesNo(place.GetBool(OzFields.Dangerous)));

            // Scene Fields store the path, so they also work in builds (the scene must be in the build list).
            string scene = place.GetSceneName(OzFields.Scene);

            Row(place, OzFields.Scene, "Scene", string.IsNullOrEmpty(scene) ? "–" : scene);

            Section("Actions");

            bool here = place == _journey.CurrentPlace;

            if (!here)
                ActionButton("Travel here", _journey.GetTravelBlocker(place), () => _journey.Travel(place));
            else if (_journey.CanRest)
                ActionButton("Rest (heal)", null, _journey.Rest);
            else
                GUILayout.Label("You are here.", S.DimLabel);

            List<Entry> items = _journey.GetItemsAt(place).ToList();

            if (items.Count == 0)
                return;

            Section("Items here");

            foreach (Entry item in items)
            {
                GUILayout.BeginHorizontal();
                ItemIcon(item);

                GUILayout.BeginVertical();
                EntryLink(item);
                GUILayout.Label($"Owner: {item.GetEntry(OzFields.Owner)?.Title ?? "nobody"}", S.DimLabel);
                GUILayout.EndVertical();

                GUI.enabled = _journey.CanPickUp(item);

                if (GUILayout.Button("Pick up", S.Button, GUILayout.Width(80f)))
                    Do(() => _journey.PickUp(item));

                GUI.enabled = true;

                GUILayout.EndHorizontal();
            }
        }

        private void DrawCharacterCard(Entry character)
        {
            Section("Character");
            Row(character, OzFields.Kind, "Kind", character.GetEnum(OzFields.Kind));
            Row(character, OzFields.Alive, "Alive", YesNo(character.GetBool(OzFields.Alive)));
            Row(character, OzFields.Health, "Health", $"{character.GetInt(OzFields.Health)}", $"{character.GetOriginalInt(OzFields.Health)}");
            Row(character, OzFields.Height, "Height", $"{character.GetFloat(OzFields.Height):0.##} m");
            Row(character, OzFields.Weaknesses, "Weaknesses", string.Join(", ", character.GetEnums(OzFields.Weaknesses)));
            Row(character, OzFields.Wish, "Wish", character.GetText(OzFields.Wish), character.GetOriginalText(OzFields.Wish));
            EntryRow(character, OzFields.Home, "Home");
            EntryRow(character, OzFields.Location, "Location");
            EntriesRow(character, OzFields.Allies, "Allies");

            DrawDialogue(character);

            string wiki = character.GetUrl(OzFields.Wiki);

            if (!string.IsNullOrEmpty(wiki) && GUILayout.Button("Open Wiki page", S.Button))
                Application.OpenURL(wiki);
        }

        // ScriptableObject Field with the demo's own type: Lore stores the reference, the game reads the lines.
        private void DrawDialogue(Entry character)
        {
            var dialogue = character.GetScriptableObject<OzDialogue>(OzFields.Dialogue);

            if (dialogue == null || dialogue.Lines.Count == 0)
                return;

            Section($"Dialogue: {dialogue.name}");

            if (_talkingTo != character)
            {
                if (GUILayout.Button("Talk", S.Button))
                    Do(() =>
                    {
                        _talkingTo = character;
                        _dialogueLine = 0;
                    });

                return;
            }

            OzDialogue.Line line = dialogue.Lines[Mathf.Clamp(_dialogueLine, 0, dialogue.Lines.Count - 1)];

            GUILayout.Label($"<b>{line.Speaker}:</b> “{line.Text}”", S.Label);

            bool last = _dialogueLine >= dialogue.Lines.Count - 1;

            if (GUILayout.Button(last ? "End" : "Next", S.Button))
                Do(() =>
                {
                    if (last)
                        _talkingTo = null;
                    else
                        _dialogueLine++;
                });
        }

        private void DrawItemCard(Entry item)
        {
            Section("Item");
            Row(item, OzFields.Rarity, "Rarity", item.GetEnum(OzFields.Rarity));
            Row(item, OzFields.Value, "Value", $"{item.GetInt(OzFields.Value)} coins");
            EntryRow(item, OzFields.Owner, "Owner");
            EntryRow(item, OzFields.Location, "Location");

            AudioClip sound = item.GetAudioClip(OzFields.PickupSound);

            if (sound != null && GUILayout.Button($"Play pickup sound ({sound.length:0.0} s)", S.Button))
                AudioSource.PlayClipAtPoint(sound, Vector3.zero);

            if (_journey.CanPickUp(item))
            {
                Section("Actions");
                ActionButton("Pick up", null, () => _journey.PickUp(item));
            }
        }

        private void DrawQuestCard(Entry quest)
        {
            Section("Quest");
            Row(quest, OzFields.Chapter, "Chapter", $"{quest.GetInt(OzFields.Chapter)}");
            Row(quest, OzFields.MainQuest, "Main Quest", YesNo(quest.GetBool(OzFields.MainQuest)));
            Row(quest, OzFields.Completed, "Completed", YesNo(quest.GetBool(OzFields.Completed)));
            EntryRow(quest, OzFields.QuestGiver, "Quest Giver");
            EntryRow(quest, OzFields.Location, "Location");
            EntriesRow(quest, OzFields.Party, "Party");
            EntriesRow(quest, OzFields.Rewards, "Rewards");
            EntryRow(quest, OzFields.NextQuest, "Next Quest");

            if (quest != _journey.CurrentQuest)
                return;

            Section("Actions");
            ActionButton("Complete Quest", _journey.GetQuestBlocker(quest), () => _journey.CompleteQuest(quest));
        }
        #endregion

        #region Side
        private void DrawSide(Rect rect)
        {
            S.Fill(rect, S.Panel);

            GUILayout.BeginArea(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, rect.height - 12f));

            DrawQuests();
            DrawInventory();
            DrawJournal();

            GUILayout.EndArea();
        }

        private void DrawQuests()
        {
            Entry current = _journey.CurrentQuest;

            GUILayout.Label("Quests", S.Heading);

            foreach (Entry quest in _journey.Quests)
            {
                Color state = quest.GetBool(OzFields.Completed) ? S.Completed : quest == current ? S.Accent : S.Upcoming;

                GUILayout.BeginHorizontal();

                Rect dot = GUILayoutUtility.GetRect(DotSize + 6f, 18f, GUILayout.ExpandWidth(false));

                S.DrawDisc(new Rect(dot.x, dot.y + 5f, DotSize, DotSize), state);
                EntryLink(quest);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Label(
                current != null
                    ? _journey.GetQuestBlocker(current) ?? $"<color=#{ColorUtility.ToHtmlStringRGB(S.Accent)}>Ready: complete “{current.Title}”.</color>"
                    : "The story is finished.",
                S.Small);

            GUILayout.Space(8f);
        }

        // Items whose Owner is the player.
        private void DrawInventory()
        {
            GUILayout.Label("Inventory", S.Heading);

            List<Entry> items = _journey.Inventory.ToList();

            if (items.Count == 0)
                GUILayout.Label("Empty.", S.DimLabel);

            GUILayout.BeginHorizontal();

            foreach (Entry item in items)
                ItemIcon(item);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        private void DrawJournal()
        {
            GUILayout.Label("Journal", S.Heading);

            _journalScroll = GUILayout.BeginScrollView(_journalScroll);

            // Indented lines come from LoreState.Changed.
            foreach (string line in _journey.Journal)
                GUILayout.Label(line, line.StartsWith(" ", StringComparison.Ordinal) ? S.DimLabel : S.Small);

            GUILayout.EndScrollView();
        }
        #endregion

        #region Rows
        private static void Section(string title)
        {
            GUILayout.Space(6f);
            GUILayout.Label(title, S.Heading);
        }

        // A Field value; an orange dot marks a value changed at runtime (IsOriginal is false), with the value from
        // Lore as tooltip when given.
        private static void Row(Entry entry, FieldKey field, string label, string value, string original = null)
        {
            GUILayout.BeginHorizontal();
            RowLabel(entry, field, label, original);
            GUILayout.Label(string.IsNullOrEmpty(value) ? "–" : value, S.Label);
            GUILayout.EndHorizontal();
        }

        private void EntryRow(Entry entry, FieldKey field, string label)
        {
            Entry value = entry.GetEntry(field);
            Entry original = entry.GetOriginalEntry(field);

            GUILayout.BeginHorizontal();
            RowLabel(entry, field, label, original != null ? original.Title : "none");

            if (value != null)
                EntryLink(value);
            else
                GUILayout.Label("–", S.Label);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void EntriesRow(Entry entry, FieldKey field, string label)
        {
            IReadOnlyList<Entry> entries = entry.GetEntries(field);

            if (entries.Count == 0)
            {
                Row(entry, field, label, null);

                return;
            }

            EntryLinks(label, entries, entry, field);
        }

        // Links that wrap into lines; the label column stays on the left.
        private void EntryLinks(string label, IReadOnlyList<Entry> entries, Entry owner = null, FieldKey field = default)
        {
            float width = CardWidth - 40f - RowLabelWidth;
            float used = 0f;

            GUILayout.BeginHorizontal();

            if (owner != null)
                RowLabel(owner, field, label, null);
            else
                GUILayout.Label(label, S.DimLabel, GUILayout.Width(RowLabelWidth));

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            for (int i = 0; i < entries.Count; i++)
            {
                string text = entries[i].Title + (i < entries.Count - 1 ? "," : string.Empty);
                float size = S.LinkButton.CalcSize(new GUIContent(text)).x + 6f;

                if (used > 0f && used + size > width)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    used = 0f;
                }

                EntryLink(entries[i], text);
                used += size;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static void RowLabel(Entry entry, FieldKey field, string label, string original)
        {
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(label), S.DimLabel, GUILayout.Width(RowLabelWidth));

            GUI.Label(rect, label, S.DimLabel);

            if (entry.IsOriginal(field))
                return;

            var dot = new Rect(rect.xMax - DotSize - 6f, rect.y + 5f, DotSize, DotSize);
            string tooltip = original != null ? $"Changed at runtime. Value in Lore: {original}" : "Changed at runtime";

            S.DrawDisc(dot, S.Changed);
            GUI.Label(Grow(dot, 3f), new GUIContent(string.Empty, tooltip));
        }

        private void EntryLink(Entry entry, string text = null)
        {
            if (GUILayout.Button(text ?? entry.Title, S.LinkButton, GUILayout.ExpandWidth(false)))
                Select(entry);
        }

        private void ItemIcon(Entry item)
        {
            Rect rect = GUILayoutUtility.GetRect(IconSize, IconSize, GUILayout.Width(IconSize), GUILayout.Height(IconSize));

            if (Event.current.type == EventType.Repaint)
                S.DrawSprite(rect, item.GetSprite(OzFields.Icon));

            if (GUI.Button(rect, new GUIContent(string.Empty, $"{item.Title} ({item.GetEnum(OzFields.Rarity)})"), GUIStyle.none))
                Select(item);
        }

        // A disabled button shows why it cannot be used.
        private void ActionButton(string text, string blocker, Action action)
        {
            GUI.enabled = blocker == null;

            if (GUILayout.Button(text, S.Button))
                Do(action);

            GUI.enabled = true;

            if (blocker != null)
                GUILayout.Label(blocker, S.DimLabel);
        }

        private static string YesNo(bool value) => value ? "Yes" : "No";
        #endregion

        #region Helpers
        private void Select(Entry entry) => Do(() =>
        {
            _selected = entry;
            _cardScroll = Vector2.zero;
        });

        private void NewGame()
        {
            _journey.NewGame();

            _selected = _journey.CurrentPlace;
            _talkingTo = null;
        }

        private void Do(Action action) => _pending = action;

        // URP expects its own data component on every camera. The scene does not contain it (it would be a missing
        // script in projects without URP), so it is added here by type name, without a URP reference.
        private static void AddRenderPipelineCameraData()
        {
            Type type = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime",
                false);

            Camera camera = Camera.main;

            if (type != null && camera != null && camera.GetComponent(type) == null)
                camera.gameObject.AddComponent(type);
        }

        private static void DrawTooltip()
        {
            if (string.IsNullOrEmpty(GUI.tooltip))
                return;

            var content = new GUIContent(GUI.tooltip);
            Vector2 size = S.Small.CalcSize(content) + new Vector2(12f, 6f);
            Vector2 mouse = Event.current.mousePosition;
            var rect = new Rect(Mathf.Min(mouse.x + 14f, ScreenWidth - size.x), mouse.y + 18f, size.x, size.y);

            S.Fill(rect, Color.black);
            GUI.Label(new Rect(rect.x + 6f, rect.y + 2f, size.x, size.y), content, S.Small);
        }
        #endregion
    }
}
