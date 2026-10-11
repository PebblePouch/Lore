using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace PebblePouch.Lore.Samples.OzDemo
{
    // The rules of the Oz Demo scene: Dorothy travels between the Places of Oz, picks up Items and completes the
    // Quests of the book. All game state lives in Lore Fields and is changed through LoreState (Set…), so the
    // Lore assets never change and one LoreState.SaveToJson() is the whole save game. OzJourneyView draws it.
    //
    // Fields are read through the generated OzFields constants (keys by Field ID: rename-safe, autocompletion).
    [RequireComponent(typeof(AudioSource))]
    public sealed class OzJourney : MonoBehaviour
    {
        #region Fields
        private const string PlayerTitle = "Dorothy Gale";
        private const string SaveFileName = "Oz Demo.json";
        private const string HomeRegion = "Kansas";
        private const string WitchKind = "Witch";
        private const int DangerDamage = 25;
        private const int JournalLength = 50;

        [SerializeField]
        private LoreProject _lore;

        private readonly List<string> _journal = new();

        private AudioSource _audio;
        private List<Entry> _places;
        private List<Entry> _items;
        private List<Entry> _quests;
        #endregion

        #region Properties
        public LoreProject Lore => _lore;

        // The player character: her Location Field is the player's position.
        public Entry Player { get; private set; }

        public IReadOnlyList<Entry> Places => _places;

        // Ordered by Chapter.
        public IReadOnlyList<Entry> Quests => _quests;

        // Newest first.
        public IReadOnlyList<string> Journal => _journal;

        public Entry CurrentPlace => Player.GetEntry(OzFields.Location);

        public IEnumerable<Entry> Inventory => _items.Where(item => item.GetEntry(OzFields.Owner) == Player);

        // The first Quest of the story that is not completed: start at the lowest Chapter, follow Next Quest.
        // Null when the story is finished.
        public Entry CurrentQuest
        {
            get
            {
                Entry quest = _quests.FirstOrDefault();

                for (int guard = 0; quest != null && guard < _quests.Count; guard++)
                {
                    if (!quest.GetBool(OzFields.Completed))
                        return quest;

                    quest = quest.GetEntry(OzFields.NextQuest);
                }

                return null;
            }
        }

        public bool HasSave => File.Exists(SavePath);

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        #endregion

        #region Unity
        private void Awake()
        {
            _audio = GetComponent<AudioSource>();

            if (_lore == null)
            {
                Debug.LogError("Oz Demo: assign the Lore Project of the Oz Demo sample.", this);
                enabled = false;

                return;
            }

            Player = _lore.FindEntryByTitle(PlayerTitle, _lore.FindCategoryByTitle("Characters"));

            _places = GetEntries("Places");
            _items = GetEntries("Items");
            _quests = GetEntries("Quests").OrderBy(quest => quest.GetInt(OzFields.Chapter)).ToList();

            if (Player == null)
            {
                Debug.LogError($"Oz Demo: the Character '{PlayerTitle}' was not found.", _lore);
                enabled = false;

                return;
            }

            AddToJournal($"{Player.Title} wakes up in {CurrentPlace?.Title ?? "nowhere"}.");
        }

        // Every runtime change of a Lore value raises Changed: the journal shows them as they happen.
        private void OnEnable() => LoreState.Changed += OnLoreChanged;

        private void OnDisable() => LoreState.Changed -= OnLoreChanged;
        #endregion

        #region Travel
        // Null when the Place can be visited, otherwise the reason why not.
        public string GetTravelBlocker(Entry place)
        {
            if (place == CurrentPlace)
                return "You are here.";

            if (place.GetEnum(OzFields.Region) == HomeRegion)
                return "Only magic can carry you home.";

            if (place.GetBool(OzFields.Dangerous) && Player.GetInt(OzFields.Health) <= DangerDamage)
                return "Too weak for a dangerous place. Rest first.";

            return null;
        }

        public void Travel(Entry place)
        {
            if (GetTravelBlocker(place) != null)
                return;

            Player.SetEntry(OzFields.Location, place);

            AddToJournal($"{Player.Title} travels to {place.Title}.");

            if (place.GetBool(OzFields.Dangerous))
            {
                Player.SetInt(OzFields.Health, Player.GetInt(OzFields.Health) - DangerDamage);

                AddToJournal($"{place.Title} is dangerous: -{DangerDamage} Health.");
            }
        }

        // Resting in a safe Place heals: the Health value goes back to the one from Lore.
        public bool CanRest =>
            CurrentPlace != null &&
            !CurrentPlace.GetBool(OzFields.Dangerous) &&
            !Player.IsOriginal(OzFields.Health);

        public void Rest()
        {
            if (!CanRest)
                return;

            Player.ResetToOriginal(OzFields.Health);

            AddToJournal($"{Player.Title} rests and feels better.");
        }
        #endregion

        #region Items
        public IEnumerable<Entry> GetItemsAt(Entry place) =>
            _items.Where(item => item.GetEntry(OzFields.Location) == place);

        // Items lying in the current Place can be taken when nobody (alive) owns them.
        public bool CanPickUp(Entry item)
        {
            Entry owner = item.GetEntry(OzFields.Owner);

            return item.GetEntry(OzFields.Location) == CurrentPlace &&
                   owner != Player &&
                   (owner == null || !owner.GetBool(OzFields.Alive));
        }

        public void PickUp(Entry item)
        {
            if (!CanPickUp(item))
                return;

            GiveItem(item, Player);
        }

        private void GiveItem(Entry item, Entry receiver)
        {
            item.SetEntry(OzFields.Owner, receiver);
            item.SetEntry(OzFields.Location, null);

            AddToJournal($"{receiver.Title} receives the {item.Title} ({item.GetEnum(OzFields.Rarity)}).");

            AudioClip sound = item.GetAudioClip(OzFields.PickupSound);

            if (sound != null)
                _audio.PlayOneShot(sound);
        }
        #endregion

        #region Quests
        // Null when the Quest can be completed now, otherwise the reason why not.
        public string GetQuestBlocker(Entry quest)
        {
            if (quest == null || quest.GetBool(OzFields.Completed))
                return "Nothing to do.";

            Entry location = quest.GetEntry(OzFields.Location);

            if (location != CurrentPlace)
                return $"Go to {location?.Title ?? "the Quest's Location"} first.";

            Entry witch = GetWickedRuler(location);

            if (witch != null && FindWeakness(witch) == null)
            {
                string weaknesses = string.Join(" or ", witch.GetEnums(OzFields.Weaknesses));

                return $"{witch.Title} rules here. Find something against her: {weaknesses}.";
            }

            return null;
        }

        public void CompleteQuest(Entry quest)
        {
            if (GetQuestBlocker(quest) != null)
                return;

            Entry location = quest.GetEntry(OzFields.Location);
            Entry witch = GetWickedRuler(location);

            quest.SetBool(OzFields.Completed, true);

            AddToJournal($"Quest completed: {quest.Title}.");

            if (witch != null)
                DefeatWitch(witch, location);

            foreach (Entry reward in quest.GetEntries(OzFields.Rewards))
                GiveReward(quest, reward);

            // The last Quest of the story: Dorothy goes home.
            if (quest.GetEntry(OzFields.NextQuest) == null)
            {
                Player.SetEntry(OzFields.Location, Player.GetEntry(OzFields.Home));
                Player.SetText(OzFields.Wish, "Granted: there is no place like home.");

                AddToJournal($"{Player.Title} knocks her heels together three times and is home again.");
            }
        }

        // A reward goes to the Party member who wishes for it (the Scarecrow's Wish mentions "Brains"), else to the
        // player. A granted Wish changes its text.
        private void GiveReward(Entry quest, Entry reward)
        {
            Entry receiver =
                quest.GetEntries(OzFields.Party)
                    .FirstOrDefault(member =>
                        member.GetText(OzFields.Wish).IndexOf(reward.Title, StringComparison.OrdinalIgnoreCase) >= 0) ??
                Player;

            if (reward.GetEntry(OzFields.Owner) == receiver)
                return;

            GiveItem(reward, receiver);

            if (receiver != Player)
                receiver.SetText(OzFields.Wish, $"Granted: {reward.Title}.");
        }

        // A living Witch ruling a dangerous Place stands in the way of every Quest there.
        private static Entry GetWickedRuler(Entry place)
        {
            Entry ruler = place != null ? place.GetEntry(OzFields.Ruler) : null;

            return ruler != null &&
                   place.GetBool(OzFields.Dangerous) &&
                   ruler.GetBool(OzFields.Alive) &&
                   ruler.GetEnum(OzFields.Kind) == WitchKind
                ? ruler
                : null;
        }

        // An Item of the player named after one of the Witch's Weaknesses ("Water" → Bucket of Water).
        private Entry FindWeakness(Entry witch)
        {
            IReadOnlyList<string> weaknesses = witch.GetEnums(OzFields.Weaknesses);

            return Inventory.FirstOrDefault(item =>
                weaknesses.Any(weakness => item.Title.IndexOf(weakness, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private void DefeatWitch(Entry witch, Entry place)
        {
            Entry weapon = FindWeakness(witch);

            witch.SetBool(OzFields.Alive, false);
            witch.SetInt(OzFields.Health, 0);

            place.SetEntry(OzFields.Ruler, null);
            place.SetBool(OzFields.Dangerous, false);

            AddToJournal($"{Player.Title} uses the {weapon.Title}: {witch.Title} melts away. {place.Title} is free.");
        }
        #endregion

        #region Save games
        // LoreState holds every change of this game (positions, owners, quests, health), keyed by IDs.
        public void Save()
        {
            File.WriteAllText(SavePath, LoreState.SaveToJson());

            AddToJournal("Game saved.");
        }

        public void Load()
        {
            if (!HasSave)
                return;

            AddToJournal(
                LoreState.LoadFromJson(File.ReadAllText(SavePath))
                    ? "Game loaded."
                    : "The save game could not be read.");
        }

        public void NewGame()
        {
            LoreState.ResetAllToOriginal();

            _journal.Clear();

            AddToJournal($"A new game. {Player.Title} wakes up in {CurrentPlace?.Title ?? "nowhere"}.");
        }
        #endregion

        #region Helpers
        private List<Entry> GetEntries(string categoryTitle)
        {
            Category category = _lore.FindCategoryByTitle(categoryTitle);

            return category != null ? _lore.GetEntries(category) : new List<Entry>();
        }

        private void OnLoreChanged(Entry entry, Field field)
        {
            // Both null: many values at once (reset all, load).
            if (entry != null && field != null)
                AddToJournal($"    {entry.Title} · {field.Title} {(entry.IsOriginal(field) ? "reset" : "changed")}");
        }

        private void AddToJournal(string line)
        {
            _journal.Insert(0, line);

            if (_journal.Count > JournalLength)
                _journal.RemoveAt(_journal.Count - 1);
        }
        #endregion
    }
}
