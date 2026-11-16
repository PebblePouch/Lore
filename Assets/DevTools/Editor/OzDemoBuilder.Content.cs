using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor.Dev
{
    // The demo's Entries: "The Wonderful Wizard of Oz" (L. Frank Baum, 1900, public domain) as the lore of a small
    // adventure game. Descriptions use the editable link form [[Title]] (converted to stored links when filled).
    //
    // Intentional issues (so Health and Integrity have something to show):
    //   Health:    Queen of the Field Mice has no Home (required), Guardian of the Gates has no Tags,
    //              Uncle Henry has no description.
    //   Yellow:    Yellow Brick Road has no Inhabitants (empty Entries list).
    //   Integrity: Glinda's Palace links [[Hammer-Heads]], an Entry nobody has written yet (broken link).
    //
    // Value types per Field type: Text/Url/Enum = string (Enum: option label), Enums = option labels, Entry = title,
    // Entries = titles, Boolean/Integer/Float = bool/int/float, Vector = Vector2, Scene = generated scene name,
    // ScriptableObject = generated dialogue name.
    // Portrait, Icon, Prefab and Pickup Sound are generated for every Character and Item, Color and Banner follow the
    // Region of every Place (see OzDemoBuilder.Assets). Map Position: Oz is 100 × 100 with the Emerald City in the
    // middle (east = +x, north = +y); Kansas is not on the map.
    // FAQ: help for demo users (no Fields). Lore names are file names (no "?"), so the title is the topic and the
    // subtitle the question; every FAQ Entry is complete (Tag "Help", notes) to add no Health issues.
    internal static partial class OzDemoBuilder
    {
        #region Types
        private sealed class EntryData
        {
            public readonly string Category;
            public readonly string Title;

            public string Subtitle = string.Empty;
            public string Glyph;
            public string Description = string.Empty;
            public string[] Tags = Array.Empty<string>();
            public Dictionary<string, object> Values = new();

            public EntryData(string category, string title)
            {
                Category = category;
                Title = title;
            }
        }
        #endregion

        #region Content
        private static readonly EntryData[] Content =
        {
            // Characters
            new("Characters", "Dorothy Gale")
            {
                Subtitle = "A little girl from Kansas",
                Glyph = "Girl",
                Tags = new[] { "Companion", "Kansas" },
                Description =
                    "Lives on the gray Kansas prairie with [[Aunt Em]], [[Uncle Henry]] and her dog [[Toto]]. A " +
                    "cyclone carries her house into the land of the Munchkins, where it falls on the [[Wicked Witch " +
                    "of the East]]. To find her way home she follows the [[Yellow Brick Road]] to the [[Emerald " +
                    "City]].\n\n*The player character.*",
                Values =
                {
                    ["Kind"] = "Human",
                    ["Home"] = "Kansas Farm",
                    ["Location"] = "Kansas Farm",
                    ["Wish"] = "To go home to Aunt Em and Uncle Henry.",
                    ["Weaknesses"] = new[] { "Poppies" },
                    ["Allies"] = new[] { "Toto", "Scarecrow", "Tin Woodman", "Cowardly Lion" },
                    ["Alive"] = true,
                    ["Health"] = 100,
                    ["Height"] = 1.3f,
                    ["Dialogue"] = "No Place Like Home",
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Dorothy_Gale",
                },
            },
            new("Characters", "Toto")
            {
                Subtitle = "Dorothy's little black dog",
                Glyph = "Dog",
                Tags = new[] { "Companion", "Animal", "Kansas" },
                Description =
                    "A small black dog with long silky hair and merry little eyes. He keeps [[Dorothy Gale]] from " +
                    "being sad, and at the end it is Toto who knocks over the screen that hides [[Oz, the Great and " +
                    "Terrible]].",
                Values =
                {
                    ["Kind"] = "Animal",
                    ["Home"] = "Kansas Farm",
                    ["Weaknesses"] = new[] { "Poppies" },
                    ["Allies"] = new[] { "Dorothy Gale" },
                    ["Alive"] = true,
                    ["Health"] = 30,
                    ["Height"] = 0.3f,
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Toto_(Oz)",
                },
            },
            new("Characters", "Scarecrow")
            {
                Subtitle = "Hopes for brains",
                Glyph = "Corn",
                Tags = new[] { "Companion", "Ruler" },
                Description =
                    "Stuffed with straw and set up in a [[Scarecrow's Cornfield]] to frighten the crows, who were " +
                    "not frightened at all. [[Dorothy Gale]] lifts him off his pole, and he joins her to ask the " +
                    "Wizard for [[Brains]]. Later he rules the [[Emerald City]].",
                Values =
                {
                    ["Kind"] = "Enchanted",
                    ["Home"] = "Scarecrow's Cornfield",
                    ["Wish"] = "Brains, so he is no longer a fool.",
                    ["Weaknesses"] = new[] { "Fire" },
                    ["Allies"] = new[] { "Dorothy Gale", "Tin Woodman", "Cowardly Lion" },
                    ["Alive"] = true,
                    ["Health"] = 60,
                    ["Height"] = 1.8f,
                    ["Dialogue"] = "Brains or a Heart",
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Scarecrow_(Oz)",
                },
            },
            new("Characters", "Tin Woodman")
            {
                Subtitle = "Hopes for a heart",
                Glyph = "Axe",
                Tags = new[] { "Companion", "Ruler" },
                Description =
                    "Once a Munchkin woodman. An enchanted axe cut him piece by piece, and a tinsmith replaced every " +
                    "part with tin, but no one could give him back his heart. Found rusted stiff in the [[Tin " +
                    "Woodman's Forest]]; he never goes anywhere without his [[Oil Can]]. Later he rules the Winkies.",
                Values =
                {
                    ["Kind"] = "Enchanted",
                    ["Home"] = "Tin Woodman's Forest",
                    ["Wish"] = "A heart, so he can love again.",
                    ["Weaknesses"] = new[] { "Rain" },
                    ["Allies"] = new[] { "Dorothy Gale", "Scarecrow", "Cowardly Lion" },
                    ["Alive"] = true,
                    ["Health"] = 120,
                    ["Height"] = 1.8f,
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Tin_Woodman",
                },
            },
            new("Characters", "Cowardly Lion")
            {
                Subtitle = "Hopes for courage",
                Glyph = "Lion",
                Tags = new[] { "Companion", "Animal", "Ruler" },
                Description =
                    "Roars at everyone so they run away, and is afraid of all of them. He carries the friends across " +
                    "the ditches of the forest but falls asleep in the [[Deadly Poppy Field]]. Later he becomes King " +
                    "of the Beasts.",
                Values =
                {
                    ["Kind"] = "Animal",
                    ["Home"] = "Tin Woodman's Forest",
                    ["Wish"] = "Courage, to be a real King of Beasts.",
                    ["Weaknesses"] = new[] { "Poppies" },
                    ["Allies"] = new[] { "Dorothy Gale", "Scarecrow", "Tin Woodman" },
                    ["Alive"] = true,
                    ["Health"] = 150,
                    ["Height"] = 1.2f,
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Cowardly_Lion",
                },
            },
            new("Characters", "Oz, the Great and Terrible")
            {
                Subtitle = "The Wizard of the Emerald City",
                Glyph = "Top Hat",
                Tags = new[] { "Ruler" },
                Description =
                    "Rules the [[Emerald City]] and appears to every visitor in a different shape. In truth he is a " +
                    "*humbug*, a balloonist from Omaha who drifted into Oz and was taken for a great wizard. He " +
                    "leaves in his [[Balloon]].",
                Values =
                {
                    ["Kind"] = "Human",
                    ["Home"] = "Emerald City",
                    ["Wish"] = "To go back to Omaha and be a circus man again.",
                    ["Allies"] = new[] { "Guardian of the Gates" },
                    ["Alive"] = true,
                    ["Health"] = 40,
                    ["Height"] = 1.6f,
                    ["Dialogue"] = "The Great and Terrible",
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Wizard_of_Oz_(character)",
                },
            },
            new("Characters", "Glinda")
            {
                Subtitle = "The Good Witch of the South",
                Glyph = "Fairy",
                Tags = new[] { "Witch", "Magic", "Ruler" },
                Description =
                    "The most powerful of the witches. She rules the Quadlings from [[Glinda's Palace]] and tells " +
                    "[[Dorothy Gale]] the secret of the [[Silver Shoes]].",
                Values =
                {
                    ["Kind"] = "Witch",
                    ["Home"] = "Glinda's Palace",
                    ["Allies"] = new[] { "Good Witch of the North" },
                    ["Alive"] = true,
                    ["Health"] = 500,
                    ["Height"] = 1.7f,
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Glinda",
                },
            },
            new("Characters", "Wicked Witch of the West")
            {
                Subtitle = "Tyrant of the Winkies",
                Glyph = "Villain",
                Tags = new[] { "Witch", "Magic", "Ruler", "Danger" },
                Description =
                    "Has only one eye, but it is as powerful as a telescope. She enslaves the Winkies and commands " +
                    "the [[King of the Winged Monkeys]] with the [[Golden Cap]]. Water is the **only** thing she " +
                    "fears, apart from the dark.",
                Values =
                {
                    ["Kind"] = "Witch",
                    ["Home"] = "Witch's Castle",
                    ["Weaknesses"] = new[] { "Water", "Darkness" },
                    ["Allies"] = new[] { "King of the Winged Monkeys" },
                    ["Alive"] = true,
                    ["Health"] = 300,
                    ["Height"] = 1.5f,
                    ["Dialogue"] = "Melting",
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Wicked_Witch_of_the_West",
                },
            },
            new("Characters", "Wicked Witch of the East")
            {
                Subtitle = "Crushed by a falling house",
                Glyph = "Boot",
                Tags = new[] { "Witch", "Magic", "Ruler" },
                Description =
                    "Held the Munchkins in bondage until [[Dorothy Gale]]'s house fell on her. Nothing is left of " +
                    "her but the [[Silver Shoes]].",
                Values =
                {
                    ["Kind"] = "Witch",
                    ["Home"] = "Munchkin Village",
                    ["Alive"] = false,
                    ["Health"] = 0,
                    ["Height"] = 1.5f,
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Wicked_Witch_of_the_East",
                },
            },
            new("Characters", "Good Witch of the North")
            {
                Subtitle = "Friend of the Munchkins",
                Glyph = "Halo",
                Tags = new[] { "Witch", "Magic" },
                Description =
                    "Greets [[Dorothy Gale]] after the house falls, gives her the [[Silver Shoes]] and a kiss on the " +
                    "forehead that no one dares to harm.",
                Values =
                {
                    ["Kind"] = "Witch",
                    ["Home"] = "Gillikin Country",
                    ["Allies"] = new[] { "Glinda" },
                    ["Alive"] = true,
                    ["Health"] = 250,
                    ["Height"] = 1.4f,
                    ["Wiki"] = "https://en.wikipedia.org/wiki/Good_Witch_of_the_North",
                },
            },
            new("Characters", "Boq")
            {
                Subtitle = "A rich Munchkin",
                Glyph = "Man",
                Tags = new[] { "Munchkin" },
                Description =
                    "Gives [[Dorothy Gale]] supper and a bed on her first night in Oz, in his blue house in the " +
                    "[[Munchkin Village]].",
                Values =
                {
                    ["Kind"] = "Human",
                    ["Home"] = "Munchkin Village",
                    ["Alive"] = true,
                    ["Health"] = 50,
                    ["Height"] = 1.1f,
                },
            },
            new("Characters", "Queen of the Field Mice")
            {
                Subtitle = "Saved from a wildcat",
                Glyph = "Mouse",
                Tags = new[] { "Animal", "Ruler" },
                // Intentional: no Home (required Field) → Health.
                Description =
                    "The [[Tin Woodman]] saves her from a wildcat. In return her mice pull the sleeping [[Cowardly " +
                    "Lion]] out of the [[Deadly Poppy Field]] on a little wooden truck.",
                Values =
                {
                    ["Kind"] = "Animal",
                    ["Alive"] = true,
                    ["Health"] = 5,
                    ["Height"] = 0.1f,
                },
            },
            new("Characters", "King of the Winged Monkeys")
            {
                Subtitle = "Bound to the Golden Cap",
                Glyph = "Monkey",
                Tags = new[] { "Animal", "Magic" },
                Description =
                    "He and his band must obey whoever owns the [[Golden Cap]], three times each. They serve the " +
                    "[[Wicked Witch of the West]] until [[Dorothy Gale]] wins the Cap.",
                Values =
                {
                    ["Kind"] = "Creature",
                    ["Home"] = "Witch's Castle",
                    ["Alive"] = true,
                    ["Health"] = 80,
                    ["Height"] = 1.0f,
                },
            },
            new("Characters", "Guardian of the Gates")
            {
                Subtitle = "Keeper of the green spectacles",
                Glyph = "Guard",
                // Intentional: no Tags → Health.
                Description =
                    "Lets visitors into the [[Emerald City]] and locks [[Green Spectacles]] on their eyes, so the " +
                    "brightness of the city will not blind them.",
                Values =
                {
                    ["Kind"] = "Human",
                    ["Home"] = "Emerald City",
                    ["Allies"] = new[] { "Oz, the Great and Terrible" },
                    ["Alive"] = true,
                    ["Health"] = 60,
                    ["Height"] = 1.2f,
                },
            },
            new("Characters", "Aunt Em")
            {
                Subtitle = "Dorothy's aunt",
                Glyph = "Older Woman",
                Tags = new[] { "Kansas" },
                Description =
                    "The sun and wind of the prairie have taken the sparkle from her eyes. She waits for [[Dorothy " +
                    "Gale]] at the [[Kansas Farm]].",
                Values =
                {
                    ["Kind"] = "Human",
                    ["Home"] = "Kansas Farm",
                    ["Allies"] = new[] { "Uncle Henry" },
                    ["Alive"] = true,
                    ["Health"] = 70,
                    ["Height"] = 1.6f,
                },
            },
            new("Characters", "Uncle Henry")
            {
                Subtitle = "Dorothy's uncle",
                Glyph = "Older Man",
                Tags = new[] { "Kansas" },
                // Intentional: no description → Health.
                Values =
                {
                    ["Kind"] = "Human",
                    ["Home"] = "Kansas Farm",
                    ["Allies"] = new[] { "Aunt Em" },
                    ["Alive"] = true,
                    ["Health"] = 70,
                    ["Height"] = 1.8f,
                },
            },

            // Places
            new("Places", "Kansas Farm")
            {
                Subtitle = "A small house on the gray prairie",
                Glyph = "House",
                Tags = new[] { "Kansas" },
                Description =
                    "One room, a cyclone cellar and gray prairie in every direction. The first and the last level of " +
                    "the game.",
                Values =
                {
                    ["Region"] = "Kansas",
                    ["Inhabitants"] = new[] { "Dorothy Gale", "Toto", "Aunt Em", "Uncle Henry" },
                    ["Dangerous"] = false,
                    ["Scene"] = "Kansas Farm",
                },
            },
            new("Places", "Munchkin Village")
            {
                Subtitle = "Where the house came down",
                Glyph = "Houses",
                Tags = new[] { "Munchkin" },
                Description =
                    "Round blue houses with domes for roofs, in the east of Oz. [[Boq]] lives here, and the [[Silver " +
                    "Shoes]] were found here.",
                Values =
                {
                    ["Region"] = "Munchkin Country",
                    ["Map Position"] = new Vector2(90f, 50f),
                    ["Ruler"] = "Wicked Witch of the East",
                    ["Inhabitants"] = new[] { "Boq" },
                    ["Dangerous"] = false,
                    ["Scene"] = "Munchkin Village",
                },
            },
            new("Places", "Yellow Brick Road")
            {
                Subtitle = "The road to the Emerald City",
                Glyph = "Footprints",
                Tags = new[] { "Munchkin" },
                // Intentional: no Inhabitants (empty Entries list) → yellow mark.
                Description =
                    "Leads from the [[Munchkin Village]] to the [[Emerald City]], past the [[Scarecrow's " +
                    "Cornfield]] and through the [[Tin Woodman's Forest]].",
                Values =
                {
                    ["Region"] = "Munchkin Country",
                    ["Map Position"] = new Vector2(75f, 50f),
                    ["Dangerous"] = true,
                },
            },
            new("Places", "Scarecrow's Cornfield")
            {
                Subtitle = "Crows welcome",
                Glyph = "Corn",
                Tags = new[] { "Munchkin" },
                Description = "A field of ripe corn beside the [[Yellow Brick Road]], guarded by the [[Scarecrow]].",
                Values =
                {
                    ["Region"] = "Munchkin Country",
                    ["Map Position"] = new Vector2(82f, 54f),
                    ["Inhabitants"] = new[] { "Scarecrow" },
                    ["Dangerous"] = false,
                },
            },
            new("Places", "Tin Woodman's Forest")
            {
                Subtitle = "Dark woods and Kalidahs",
                Glyph = "Evergreen Tree",
                Tags = new[] { "Danger" },
                Description =
                    "Thick woods where the [[Tin Woodman]] stood rusted for a year, and where the [[Cowardly Lion]] " +
                    "jumps out at the travelers. Beyond the great ditches live the Kalidahs.",
                Values =
                {
                    ["Region"] = "Munchkin Country",
                    ["Map Position"] = new Vector2(68f, 47f),
                    ["Inhabitants"] = new[] { "Tin Woodman", "Cowardly Lion" },
                    ["Dangerous"] = true,
                },
            },
            new("Places", "Deadly Poppy Field")
            {
                Subtitle = "Sleep forever",
                Glyph = "Blossom",
                Tags = new[] { "Danger", "Magic" },
                Description =
                    "Scarlet poppies whose scent puts anyone who breathes it to sleep. The [[Scarecrow]] and the " +
                    "[[Tin Woodman]] do not breathe, so they carry [[Dorothy Gale]] out.",
                Values =
                {
                    ["Region"] = "Emerald City",
                    ["Map Position"] = new Vector2(58f, 52f),
                    ["Inhabitants"] = new[] { "Queen of the Field Mice" },
                    ["Dangerous"] = true,
                },
            },
            new("Places", "Emerald City")
            {
                Subtitle = "Everything is green",
                Glyph = "Cityscape",
                Tags = new[] { "Magic" },
                Description =
                    "The city of [[Oz, the Great and Terrible]], glowing green behind its walls. Everybody wears " +
                    "[[Green Spectacles]] here, which may be why it looks so green.",
                Values =
                {
                    ["Region"] = "Emerald City",
                    ["Map Position"] = new Vector2(50f, 50f),
                    ["Ruler"] = "Oz, the Great and Terrible",
                    ["Inhabitants"] = new[] { "Oz, the Great and Terrible", "Guardian of the Gates" },
                    ["Dangerous"] = false,
                    ["Scene"] = "Emerald City",
                },
            },
            new("Places", "Witch's Castle")
            {
                Subtitle = "In the yellow country of the Winkies",
                Glyph = "Castle",
                Tags = new[] { "Danger" },
                Description =
                    "The yellow castle of the [[Wicked Witch of the West]], in the west of Oz. The final dungeon of " +
                    "the game.",
                Values =
                {
                    ["Region"] = "Winkie Country",
                    ["Map Position"] = new Vector2(12f, 55f),
                    ["Ruler"] = "Wicked Witch of the West",
                    ["Inhabitants"] = new[] { "Wicked Witch of the West", "King of the Winged Monkeys" },
                    ["Dangerous"] = true,
                    ["Scene"] = "Witch's Castle",
                },
            },
            new("Places", "Glinda's Palace")
            {
                Subtitle = "In the red country of the Quadlings",
                Glyph = "Japanese Castle",
                Tags = new[] { "Magic" },
                // Intentional: [[Hammer-Heads]] has no Entry yet → broken link (Integrity).
                Description =
                    "Home of [[Glinda]], in the south of Oz. The road there leads over the hill of the " +
                    "[[Hammer-Heads]], who do not let anyone pass.",
                Values =
                {
                    ["Region"] = "Quadling Country",
                    ["Map Position"] = new Vector2(50f, 10f),
                    ["Ruler"] = "Glinda",
                    ["Inhabitants"] = new[] { "Glinda" },
                    ["Dangerous"] = false,
                },
            },
            new("Places", "Gillikin Country")
            {
                Subtitle = "The purple north",
                Glyph = "Snowy Mountain",
                Tags = new[] { "Magic" },
                Description = "The north of Oz, home of the [[Good Witch of the North]].",
                Values =
                {
                    ["Region"] = "Gillikin Country",
                    ["Map Position"] = new Vector2(50f, 90f),
                    ["Ruler"] = "Good Witch of the North",
                    ["Inhabitants"] = new[] { "Good Witch of the North" },
                    ["Dangerous"] = false,
                },
            },

            // Items
            new("Items", "Silver Shoes")
            {
                Subtitle = "Three knocks of the heels",
                Glyph = "Boot",
                Tags = new[] { "Magic" },
                Description =
                    "Once worn by the [[Wicked Witch of the East]]. Whoever knocks the heels together three times " +
                    "can go anywhere in three steps. Only [[Glinda]] knows their secret.",
                Values =
                {
                    ["Rarity"] = "Unique",
                    ["Owner"] = "Wicked Witch of the East",
                    ["Location"] = "Munchkin Village",
                    ["Value"] = 1000,
                },
            },
            new("Items", "Golden Cap")
            {
                Subtitle = "Three wishes of the Winged Monkeys",
                Glyph = "Crown",
                Tags = new[] { "Magic" },
                Description =
                    "Its owner can call the Winged Monkeys three times. The [[King of the Winged Monkeys]] tells its " +
                    "story.",
                Values =
                {
                    ["Rarity"] = "Unique",
                    ["Owner"] = "Wicked Witch of the West",
                    ["Location"] = "Witch's Castle",
                    ["Value"] = 800,
                },
            },
            new("Items", "Green Spectacles")
            {
                Subtitle = "Locked on with a golden key",
                Glyph = "Glasses",
                Tags = new[] { "Gift" },
                Description = "Handed out by the [[Guardian of the Gates]] to everyone who enters the [[Emerald City]].",
                Values =
                {
                    ["Rarity"] = "Common",
                    ["Owner"] = "Guardian of the Gates",
                    ["Location"] = "Emerald City",
                    ["Value"] = 5,
                },
            },
            new("Items", "Oil Can")
            {
                Subtitle = "Against rust",
                Glyph = "Oil Barrel",
                Tags = new[] { "Tool" },
                Description = "Keeps the joints of the [[Tin Woodman]] moving. Rain is his worst enemy.",
                Values =
                {
                    ["Rarity"] = "Common",
                    ["Owner"] = "Tin Woodman",
                    ["Location"] = "Tin Woodman's Forest",
                    ["Value"] = 3,
                },
            },
            new("Items", "Bucket of Water")
            {
                Subtitle = "Thrown in anger",
                Glyph = "Bucket",
                Tags = new[] { "Tool" },
                Description =
                    "[[Dorothy Gale]] throws it over the [[Wicked Witch of the West]] when the Witch steals one of " +
                    "the [[Silver Shoes]].",
                Values =
                {
                    ["Rarity"] = "Common",
                    ["Location"] = "Witch's Castle",
                    ["Value"] = 1,
                },
            },
            new("Items", "Brains")
            {
                Subtitle = "Bran, pins and needles",
                Glyph = "Light Bulb",
                Tags = new[] { "Gift" },
                Description =
                    "What [[Oz, the Great and Terrible]] puts into the head of the [[Scarecrow]]. They make him feel " +
                    "very wise.",
                Values =
                {
                    ["Rarity"] = "Magical",
                    ["Owner"] = "Oz, the Great and Terrible",
                    ["Location"] = "Emerald City",
                    ["Value"] = 0,
                },
            },
            new("Items", "Heart")
            {
                Subtitle = "Silk, stuffed with sawdust",
                Glyph = "Heart",
                Tags = new[] { "Gift" },
                Description = "A pretty heart for the [[Tin Woodman]]. He is sure it is a kind one.",
                Values =
                {
                    ["Rarity"] = "Magical",
                    ["Owner"] = "Oz, the Great and Terrible",
                    ["Location"] = "Emerald City",
                    ["Value"] = 0,
                },
            },
            new("Items", "Courage")
            {
                Subtitle = "A green drink",
                Glyph = "Potion",
                Tags = new[] { "Gift" },
                Description = "Poured from a square green bottle for the [[Cowardly Lion]]. Courage is always inside.",
                Values =
                {
                    ["Rarity"] = "Magical",
                    ["Owner"] = "Oz, the Great and Terrible",
                    ["Location"] = "Emerald City",
                    ["Value"] = 0,
                },
            },
            new("Items", "Balloon")
            {
                Subtitle = "Made of green silk",
                Glyph = "Balloon",
                Tags = new[] { "Tool" },
                Description =
                    "Sewn from strips of silk, it carries [[Oz, the Great and Terrible]] away from the [[Emerald " +
                    "City]]. It leaves without [[Dorothy Gale]].",
                Values =
                {
                    ["Rarity"] = "Rare",
                    ["Owner"] = "Oz, the Great and Terrible",
                    ["Location"] = "Emerald City",
                    ["Value"] = 500,
                },
            },

            // Quests (main story, chained through Next Quest)
            new("Quests", "The Council with the Munchkins")
            {
                Subtitle = "Chapter 2",
                Tags = new[] { "Munchkin", "Witch" },
                Description =
                    "[[Dorothy Gale]] wakes up in the land of the Munchkins. The [[Good Witch of the North]] sends " +
                    "her to the Wizard and gives her the [[Silver Shoes]].",
                Values =
                {
                    ["Chapter"] = 2,
                    ["Quest Giver"] = "Good Witch of the North",
                    ["Party"] = new[] { "Dorothy Gale", "Toto" },
                    ["Location"] = "Munchkin Village",
                    ["Rewards"] = new[] { "Silver Shoes" },
                    ["Next Quest"] = "Follow the Yellow Brick Road",
                    ["Main Quest"] = true,
                },
            },
            new("Quests", "Follow the Yellow Brick Road")
            {
                Subtitle = "Chapters 3 to 7",
                Tags = new[] { "Companion" },
                Description =
                    "On the way to the [[Emerald City]], [[Dorothy Gale]] meets the [[Scarecrow]], the [[Tin " +
                    "Woodman]] and the [[Cowardly Lion]].",
                Values =
                {
                    ["Chapter"] = 3,
                    ["Quest Giver"] = "Good Witch of the North",
                    ["Party"] = new[] { "Dorothy Gale", "Toto", "Scarecrow", "Tin Woodman", "Cowardly Lion" },
                    ["Location"] = "Yellow Brick Road",
                    ["Rewards"] = new[] { "Oil Can" },
                    ["Next Quest"] = "Escape the Poppy Field",
                    ["Main Quest"] = true,
                },
            },
            new("Quests", "Escape the Poppy Field")
            {
                Subtitle = "Chapters 8 and 9",
                Tags = new[] { "Danger", "Animal" },
                Description =
                    "Get the sleeping friends out of the [[Deadly Poppy Field]], with the help of the [[Queen of " +
                    "the Field Mice]].",
                Values =
                {
                    ["Chapter"] = 8,
                    ["Quest Giver"] = "Tin Woodman",
                    ["Party"] = new[] { "Scarecrow", "Tin Woodman", "Queen of the Field Mice" },
                    ["Location"] = "Deadly Poppy Field",
                    ["Next Quest"] = "Meet the Wizard",
                    ["Main Quest"] = false,
                },
            },
            new("Quests", "Meet the Wizard")
            {
                Subtitle = "Chapters 10 and 11",
                Tags = new[] { "Ruler" },
                Description =
                    "Each friend sees [[Oz, the Great and Terrible]] alone, and each sees him in a different shape. " +
                    "His price: kill the [[Wicked Witch of the West]].",
                Values =
                {
                    ["Chapter"] = 10,
                    ["Quest Giver"] = "Guardian of the Gates",
                    ["Party"] = new[] { "Dorothy Gale", "Toto", "Scarecrow", "Tin Woodman", "Cowardly Lion" },
                    ["Location"] = "Emerald City",
                    ["Rewards"] = new[] { "Green Spectacles" },
                    ["Next Quest"] = "Destroy the Wicked Witch",
                    ["Main Quest"] = true,
                },
            },
            new("Quests", "Destroy the Wicked Witch")
            {
                Subtitle = "Chapters 12 to 14",
                Tags = new[] { "Witch", "Danger" },
                Description =
                    "Travel west to the [[Witch's Castle]]. The Witch sends wolves, crows, bees and finally the " +
                    "Winged Monkeys; a [[Bucket of Water]] ends it.",
                Values =
                {
                    ["Chapter"] = 12,
                    ["Quest Giver"] = "Oz, the Great and Terrible",
                    ["Party"] = new[] { "Dorothy Gale", "Toto", "Scarecrow", "Tin Woodman", "Cowardly Lion" },
                    ["Location"] = "Witch's Castle",
                    ["Rewards"] = new[] { "Golden Cap" },
                    ["Next Quest"] = "The Gifts of the Wizard",
                    ["Main Quest"] = true,
                },
            },
            new("Quests", "The Gifts of the Wizard")
            {
                Subtitle = "Chapters 15 and 16",
                Tags = new[] { "Gift" },
                Description =
                    "Back in the [[Emerald City]], the Wizard turns out to be a humbug. He keeps his promises " +
                    "anyway: [[Brains]], a [[Heart]] and [[Courage]].",
                Values =
                {
                    ["Chapter"] = 15,
                    ["Quest Giver"] = "Oz, the Great and Terrible",
                    ["Party"] = new[] { "Scarecrow", "Tin Woodman", "Cowardly Lion" },
                    ["Location"] = "Emerald City",
                    ["Rewards"] = new[] { "Brains", "Heart", "Courage" },
                    ["Next Quest"] = "Return to Kansas",
                    ["Main Quest"] = true,
                },
            },
            new("Quests", "Return to Kansas")
            {
                Subtitle = "Chapters 18 to 24",
                Tags = new[] { "Kansas", "Magic" },
                Description =
                    "The [[Balloon]] leaves without her, so [[Dorothy Gale]] travels south to [[Glinda]], who knows " +
                    "what the [[Silver Shoes]] can do.",
                Values =
                {
                    ["Chapter"] = 18,
                    ["Quest Giver"] = "Glinda",
                    ["Party"] = new[] { "Dorothy Gale", "Toto", "Scarecrow", "Tin Woodman", "Cowardly Lion" },
                    ["Location"] = "Glinda's Palace",
                    ["Rewards"] = new[] { "Silver Shoes" },
                    ["Main Quest"] = true,
                },
            },

            // FAQ (no Fields; title = question, subtitle = short answer, description = full answer)
            new("FAQ", "Starting Fresh")
            {
                Subtitle = "How can I start fresh with my own Lore?",
                Glyph = "Seedling",
                Tags = new[] { "Help" },
                Description =
                    "A Unity project holds **one** Lore Project, and right now that is this demo. Delete the sample " +
                    "folder Assets/Samples/Lore (it contains the whole demo), then open Tools > Lore > Open Lore: " +
                    "the Welcome page lets you start your own Lore.\n\nYou can import the demo again at any time " +
                    "from Package Manager > Lore > Samples, but only while you have no Lore Project of your own.",
            },
            new("FAQ", "Yellow and Red Marks")
            {
                Subtitle = "Why are some Entries marked yellow or red?",
                Glyph = "Warning",
                Tags = new[] { "Help" },
                Description =
                    "**Yellow** means unfinished content (Health), **red** means broken data (Integrity). The demo " +
                    "contains a few of both on purpose: [[Queen of the Field Mice]] has no Home although it is " +
                    "required, [[Guardian of the Gates]] has no Tags, [[Uncle Henry]] has no notes, the [[Yellow " +
                    "Brick Road]] has no Inhabitants, and [[Glinda's Palace]] links to the Hammer-Heads, an Entry " +
                    "nobody has written yet.\n\nClick a Health or Integrity item on the Project page to show the " +
                    "affected Entries in the Contents.",
            },
            new("FAQ", "Playing the Demo")
            {
                Subtitle = "How do I play the demo?",
                Glyph = "Joystick",
                Tags = new[] { "Help" },
                Description =
                    "Open Scenes/Oz Demo.unity in the sample folder and press Play. [[Dorothy Gale]] starts at the " +
                    "[[Kansas Farm]]; click a place on the map to travel, pick up Items and complete the Quests of " +
                    "the book.\n\nThe whole game state lives in Lore Fields, changed at runtime through the Runtime " +
                    "API. Values that differ from the Lore data are marked in the view.",
            },
            new("FAQ", "Save Games")
            {
                Subtitle = "Where is my save game?",
                Glyph = "Floppy Disk",
                Tags = new[] { "Help" },
                Description =
                    "Save writes all runtime changes, as returned by LoreState.SaveToJson, to Oz Demo.json in " +
                    "Application.persistentDataPath (on Windows: AppData/LocalLow/YourCompany/YourProduct). Load reads " +
                    "it back, New Game discards all changes.\n\nThe Lore assets themselves never change while the " +
                    "game runs, so the Lore window always shows the start of the game.",
            },
            new("FAQ", "Editing the Demo")
            {
                Subtitle = "Can I edit the demo?",
                Glyph = "Pencil",
                Tags = new[] { "Help" },
                Description =
                    "The demo is a normal Lore Project: rename Entries, add Fields, change values and see how the " +
                    "game reacts. Importing the sample again replaces your changes with the original demo.",
            },
            new("FAQ", "Undoing Deletes")
            {
                Subtitle = "Why can't I undo a deleted Entry or Category?",
                Glyph = "Wastebasket",
                Tags = new[] { "Help" },
                Description =
                    "Entries and Categories are asset files. Creating, duplicating, renaming and deleting them are " +
                    "file operations, and like in Unity's Project window they cannot be undone. Lore always asks " +
                    "before it deletes something and tells you how many references will break.\n\nEdits of values, " +
                    "notes, Tags and Fields **can** be undone with Ctrl+Z. To be safe against deleting by mistake, " +
                    "keep your project in version control (e.g. Git or Unity Version Control) and commit regularly.",
            },
            new("FAQ", "Hotkeys")
            {
                Subtitle = "Which hotkeys does Lore support, and can I change them?",
                Glyph = "Keyboard",
                Tags = new[] { "Help" },
                Description =
                    "**Ctrl+F / Cmd+F** Quick Search, **Alt+Left / Backspace** and **Alt+Right** back and forward, " +
                    "**arrow keys** move through the Contents (Left jumps back to them), **Insert** new Category, " +
                    "new Entry or duplicate, **F2** rename, **Delete** delete (asks first), **+** open the matching " +
                    "Library, **Page Up / Page Down / Home / End** scroll the page, **Escape** finish editing, clear " +
                    "the search or close a Library.\n\nThe Lore hotkeys are fixed.",
            },
            new("FAQ", "Text Formatting")
            {
                Subtitle = "Is BB code supported, and where?",
                Glyph = "Memo",
                Tags = new[] { "Help" },
                Description =
                    "Lore has no BB code. Its descriptions and notes support a small Markdown-like syntax instead: " +
                    "two asterisks or underscores around a text make it **bold**, one makes it *italic*, and two " +
                    "opening square brackets insert a link to an Entry, like [[Toto]].\n\nThis works in the " +
                    "descriptions of the Project and Categories and in the notes of Entries; Text Fields stay plain " +
                    "text. In your game, LoreProject.FormatDescription turns the formatting into Unity rich text.",
            },
            new("FAQ", "Runtime Scripts")
            {
                Subtitle = "Where are the scripts to access Lore data at runtime?",
                Glyph = "Toolbox",
                Tags = new[] { "Help" },
                Description =
                    "The Runtime API comes with the package (Packages/Lore/Runtime); there is nothing to copy. Give " +
                    "a MonoBehaviour a field of type LoreProject or Entry, assign the asset, and read values, e.g. " +
                    "entry.GetInt(\"Health\").\n\nExamples: the Scripts folder of this demo (OzJourney.cs has the " +
                    "game rules, Generated/OzFields.cs the generated Field constants) and the Runtime API sample " +
                    "(Package Manager > Lore > Samples). The full reference is Documentation~/RuntimeAPI.md in the " +
                    "package.",
            },
            new("FAQ", "Build Size")
            {
                Subtitle = "Does Lore data bloat my game build?",
                Glyph = "Package",
                Tags = new[] { "Help" },
                Description =
                    "The Lore window and all editor tools never end up in a build. A build only contains Lore data " +
                    "that a scene references: a LoreProject reference brings in all Entries, a single Entry " +
                    "reference only that Entry. Entries themselves are mostly text and small.\n\nWhat can get big " +
                    "are the assets field values point to (Sprites, prefabs, audio clips): they load together with " +
                    "the Entry. Reference the Project only where you need lookups, and single Entries elsewhere. " +
                    "Details: \"Memory & loading\" in Documentation~/RuntimeAPI.md.",
            },
        };
        #endregion
    }
}
