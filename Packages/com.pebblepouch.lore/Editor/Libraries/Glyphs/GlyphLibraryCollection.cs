using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class GlyphLibraryCollection : ScriptableObject
    {
        #region Fields
        // Raise whenever the default glyphs change: older assets then offer a Library update (LibraryUpdateService).
        public const int CatalogVersion = 1;

        [SerializeField, HideInInspector]
        private List<GlyphDefinition> _glyphs = new();

        // Catalog version the asset was generated or last updated with (0 = before catalog versions existed).
        [SerializeField, HideInInspector]
        private int _catalogVersion;
        #endregion

        #region Properties
        public IReadOnlyList<GlyphDefinition> Glyphs => _glyphs;

        public bool IsCatalogOutdated => _catalogVersion < CatalogVersion;
        #endregion

        #region Catalog
        public void MarkCatalogCurrent()
        {
            _catalogVersion = CatalogVersion;
        }

        // Adds the default glyphs this asset does not have yet (matched by glyph string) at their default position;
        // existing glyphs stay unchanged, glyphs that are not (or no longer) defaults move to the end. Returns how
        // many were added.
        public int AppendMissingDefaults()
        {
            GlyphLibraryCollection defaults = CreateInstance<GlyphLibraryCollection>();

            try
            {
                defaults.GenerateDefaultCollection();

                Dictionary<string, GlyphDefinition> existing = new();

                foreach (GlyphDefinition glyph in _glyphs)
                    existing.TryAdd(glyph.Glyph, glyph);

                List<GlyphDefinition> merged = new();
                HashSet<string> defaultGlyphs = new();
                int added = 0;

                foreach (GlyphDefinition glyph in defaults._glyphs)
                {
                    if (!defaultGlyphs.Add(glyph.Glyph))
                        continue;

                    if (existing.TryGetValue(glyph.Glyph, out GlyphDefinition own))
                    {
                        merged.Add(own);
                    }
                    else
                    {
                        merged.Add(glyph);
                        added++;
                    }
                }

                foreach (GlyphDefinition glyph in _glyphs)
                {
                    if (!defaultGlyphs.Contains(glyph.Glyph))
                        merged.Add(glyph);
                }

                _glyphs = merged;

                MarkCatalogCurrent();

                return added;
            }
            finally
            {
                DestroyImmediate(defaults);
            }
        }
        #endregion

        #region Helpers
        private void Add(
            string glyph,
            string title,
            string category)
        {
            _glyphs.Add(
                new GlyphDefinition
                {
                    Glyph = glyph,
                    Title = title,
                    Category = category
                });
        }

#if UNITY_EDITOR
        // Order: People, Creatures, Places, Nature, Things, Food & Drink, Mobility, Symbols. Within a group: base form
        // first, then variants (hand-picked, not alphabetical). Single code points only, always with \uFE0F.
        public void GenerateDefaultCollection()
        {
            _glyphs.Clear();

            MarkCatalogCurrent();

            // Glyphs: https://unicode.org/emoji/charts/full-emoji-list.html

            // ===== People =====

            // Characters
            Add("\U0001F464\uFE0F", "Silhouette", "Character, Abstract");
            Add("\U0001F465\uFE0F", "Group", "Character, Abstract");
            Add("\U0001F5E3\uFE0F", "Speaking Head", "Character, Abstract");

            Add("\U0001F476\uFE0F", "Baby", "Character");
            Add("\U0001F9D2\uFE0F", "Child", "Character");
            Add("\U0001F466\uFE0F", "Boy", "Character");
            Add("\U0001F467\uFE0F", "Girl", "Character");
            Add("\U0001F9D1\uFE0F", "Person", "Character");
            Add("\U0001F468\uFE0F", "Man", "Character");
            Add("\U0001F469\uFE0F", "Woman", "Character");
            Add("\U0001F9D4\uFE0F", "Bearded Person", "Character");
            Add("\U0001F9D3\uFE0F", "Older Person", "Character");
            Add("\U0001F474\uFE0F", "Older Man", "Character");
            Add("\U0001F475\uFE0F", "Older Woman", "Character");
            Add("\U0001F46A\uFE0F", "Family", "Character");
            Add("\U0001FAC2\uFE0F", "People Hugging", "Character");
            Add("\U0001F46B\uFE0F", "Couple", "Character");
            Add("\U0001F491\uFE0F", "Couple with Heart", "Character");
            Add("\U0001F48F\uFE0F", "Couple Kiss", "Character");

            // Character Themes
            Add("\U0001F9B8\uFE0F", "Hero", "Character, Fantasy");
            Add("\U0001F9B9\uFE0F", "Villain", "Character, Fantasy");
            Add("\U0001FAC5\uFE0F", "Monarch", "Character, Fantasy");
            Add("\U0001F934\uFE0F", "Prince", "Character, Fantasy");
            Add("\U0001F478\uFE0F", "Princess", "Character, Fantasy");
            Add("\U0001F9D9\uFE0F", "Mage", "Character, Fantasy");
            Add("\U0001F9DD\uFE0F", "Elf", "Character, Fantasy");
            Add("\U0001F9CC\uFE0F", "Troll", "Character, Fantasy");
            Add("\U0001F479\uFE0F", "Ogre", "Character, Fantasy");
            Add("\U0001F47A\uFE0F", "Goblin", "Character, Fantasy");
            Add("\U0001F9DA\uFE0F", "Fairy", "Character, Fantasy");
            Add("\U0001F9DC\uFE0F", "Merperson", "Character, Fantasy");
            Add("\U0001F9DE\uFE0F", "Genie", "Character, Fantasy");
            Add("\U0001F608\uFE0F", "Devil", "Character, Fantasy");
            Add("\U0001F47F\uFE0F", "Imp", "Character, Fantasy");
            Add("\U0001F47C\uFE0F", "Angel", "Character, Fantasy");

            Add("\U0001F47D\uFE0F", "Alien", "Character, Sci-Fi");
            Add("\U0001F47E\uFE0F", "Alien Monster", "Character, Sci-Fi");
            Add("\U0001F916\uFE0F", "Robot", "Character, Sci-Fi");

            Add("\U0001F47B\uFE0F", "Ghost", "Character, Horror");
            Add("\U0001F9DF\uFE0F", "Zombie", "Character, Horror");
            Add("\U0001F9DB\uFE0F", "Vampire", "Character, Horror");

            Add("\U0001F921\uFE0F", "Clown", "Character, Leisure");
            Add("\U0001F385\uFE0F", "Mr. Santa", "Character, Leisure");
            Add("\U0001F936\uFE0F", "Mrs. Santa", "Character, Leisure");

            // Character Roles
            Add("\U0001F46E\uFE0F", "Officer", "Character, Role");
            Add("\U0001F575\uFE0F", "Detective", "Character, Role");
            Add("\U0001F482\uFE0F", "Guard", "Character, Role");
            Add("\U0001F977\uFE0F", "Ninja", "Character, Role");
            Add("\U0001F93A\uFE0F", "Fencer", "Character, Role");
            Add("\U0001F920\uFE0F", "Cowboy", "Character, Role");
            Add("\U0001F477\uFE0F", "Worker", "Character, Role");

            // Character Activities
            Add("\U0001F9CD\uFE0F", "Standing", "Character, Activity");
            Add("\U0001F9CE\uFE0F", "Kneeling", "Character, Activity");
            Add("\U0001F6B6\uFE0F", "Walking", "Character, Activity");
            Add("\U0001F3C3\uFE0F", "Running", "Character, Activity");
            Add("\U0001F483\uFE0F", "Dancer", "Character, Activity");
            Add("\U0001F57A\uFE0F", "Dancing Man", "Character, Activity");
            Add("\U0001F9D8\uFE0F", "Meditating", "Character, Activity");
            Add("\U0001F647\uFE0F", "Bowing", "Character, Activity");
            Add("\U0001F937\uFE0F", "Shrugging", "Character, Activity");
            Add("\U0001F9D7\uFE0F", "Climbing", "Character, Activity");
            Add("\U0001F3CA\uFE0F", "Swimming", "Character, Activity");
            Add("\U0001F6A3\uFE0F", "Rowing", "Character, Activity");
            Add("\U0001F3C7\uFE0F", "Horse Riding", "Character, Activity");
            Add("\U0001F6C0\uFE0F", "Bathing", "Character, Activity");
            Add("\U0001F6CC\uFE0F", "In Bed", "Character, Activity");

            // Character Outfits
            Add("\U0001F473\uFE0F", "Turban", "Character, Outfit");
            Add("\U0001F472\uFE0F", "Skullcap", "Character, Outfit");
            Add("\U0001F9D5\uFE0F", "Headscarf", "Character, Outfit");
            Add("\U0001F935\uFE0F", "Tuxedo", "Character, Outfit");
            Add("\U0001F470\uFE0F", "Veil", "Character, Outfit");

            // Clothes
            Add("\U0001F455\uFE0F", "Shirt", "Clothes");
            Add("\U0001F457\uFE0F", "Dress", "Clothes");
            Add("\U0001F9E5\uFE0F", "Coat", "Clothes");
            Add("\U0001F459\uFE0F", "Bikini", "Clothes");
            Add("\U0001FA72\uFE0F", "Panties", "Clothes");
            Add("\U0001F454\uFE0F", "Necktie", "Clothes");
            Add("\U0001F456\uFE0F", "Jeans", "Clothes");
            Add("\U0001F45A\uFE0F", "Blouse", "Clothes");
            Add("\U0001FA73\uFE0F", "Shorts", "Clothes");
            Add("\U0001FA71\uFE0F", "Swimsuit", "Clothes");
            Add("\U0001F458\uFE0F", "Kimono", "Clothes");
            Add("\U0001F97B\uFE0F", "Sari", "Clothes");
            Add("\U0001F97C\uFE0F", "Lab Coat", "Clothes");
            Add("\U0001F9BA\uFE0F", "Safety Vest", "Clothes");
            Add("\U0001F9E3\uFE0F", "Scarf", "Clothes");
            Add("\U0001F9E4\uFE0F", "Gloves", "Clothes");
            Add("\U0001F9E6\uFE0F", "Socks", "Clothes");
            Add("\U0001F462\uFE0F", "Boot", "Clothes");
            Add("\U0001F45E\uFE0F", "Shoe", "Clothes");
            Add("\U0001F45F\uFE0F", "Sneaker", "Clothes");
            Add("\U0001F97E\uFE0F", "Hiking Boot", "Clothes");
            Add("\U0001F97F\uFE0F", "Flat Shoe", "Clothes");
            Add("\U0001F460\uFE0F", "High Heel", "Clothes");
            Add("\U0001F461\uFE0F", "Sandal", "Clothes");
            Add("\U0001FA74\uFE0F", "Thong Sandal", "Clothes");
            Add("\U0001FA70\uFE0F", "Ballet Shoes", "Clothes");

            Add("\U0001F452\uFE0F", "Hat", "Clothes, Headwear");
            Add("\U0001F3A9\uFE0F", "Top Hat", "Clothes, Headwear");
            Add("\U000026D1\uFE0F", "Rescue Helmet", "Clothes, Headwear");
            Add("\U0001FA96\uFE0F", "Military Helmet", "Clothes, Headwear");
            Add("\U0001F9E2\uFE0F", "Billed Cap", "Clothes, Headwear");
            Add("\U0001F451\uFE0F", "Crown", "Clothes, Fantasy");

            Add("\U0001F453\uFE0F", "Glasses", "Clothes, Accessory");
            Add("\U0001F576\uFE0F", "Sunglasses", "Clothes, Accessory");
            Add("\U0001F97D\uFE0F", "Goggles", "Clothes, Accessory");
            Add("\U0001F392\uFE0F", "Backpack", "Clothes, Accessory");
            Add("\U0001F9F3\uFE0F", "Luggage", "Clothes, Accessory");
            Add("\U00002602\uFE0F", "Umbrella", "Clothes, Accessory");
            Add("\U0001F45B\uFE0F", "Purse", "Clothes, Accessory");
            Add("\U0001F45C\uFE0F", "Handbag", "Clothes, Accessory");
            Add("\U0001F45D\uFE0F", "Clutch Bag", "Clothes, Accessory");
            Add("\U0001F6CD\uFE0F", "Shopping Bags", "Clothes, Accessory");
            Add("\U0001F484\uFE0F", "Lipstick", "Clothes, Accessory");
            Add("\U0001FAAD\uFE0F", "Folding Fan", "Clothes, Accessory");
            Add("\U0001FAAE\uFE0F", "Hair Pick", "Clothes, Accessory");

            // Body
            Add("\U0001F441\uFE0F", "Eye", "Body");
            Add("\U0001F440\uFE0F", "Eyes", "Body");
            Add("\U0001F442\uFE0F", "Ear", "Body");
            Add("\U0001F443\uFE0F", "Nose", "Body");
            Add("\U0001F444\uFE0F", "Mouth", "Body");
            Add("\U0001F445\uFE0F", "Tongue", "Body");
            Add("\U0001F9B7\uFE0F", "Tooth", "Body");
            Add("\U0001F4AA\uFE0F", "Arm", "Body");
            Add("\U0001F9B5\uFE0F", "Leg", "Body");
            Add("\U0001F9B6\uFE0F", "Foot", "Body");
            Add("\U0001F9BE\uFE0F", "Mechanical Arm", "Body");
            Add("\U0001F9BF\uFE0F", "Mechanical Leg", "Body");
            Add("\U0001F463\uFE0F", "Footprints", "Body");
            Add("\U0001FAC6\uFE0F", "Fingerprint", "Body");
            Add("\U0001F9E0\uFE0F", "Brain", "Body");
            Add("\U0001FAC0\uFE0F", "Anatomical Heart", "Body");
            Add("\U0001FAC1\uFE0F", "Lungs", "Body");
            Add("\U0001FA78\uFE0F", "Blood Drop", "Body");

            Add("\U0000270B\uFE0F", "Raise", "Body, Hand");
            Add("\U0001F448\uFE0F", "Left", "Body, Hand");
            Add("\U0001F449\uFE0F", "Right", "Body, Hand");
            Add("\U0001F446\uFE0F", "Up", "Body, Hand");
            Add("\U0001F447\uFE0F", "Down", "Body, Hand");
            Add("\U0001F44D\uFE0F", "Yes", "Body, Hand");
            Add("\U0001F44E\uFE0F", "No", "Body, Hand");
            Add("\U0001FAF5\uFE0F", "You", "Body, Hand");
            Add("\U0001F44A\uFE0F", "Fist", "Body, Hand");
            Add("\U0001FAF3\uFE0F", "Give", "Body, Hand");
            Add("\U0001FAF4\uFE0F", "Receive", "Body, Hand");
            Add("\U0001F44B\uFE0F", "Wave", "Body, Hand");
            Add("\U0000270A\uFE0F", "Raised Fist", "Body, Hand");
            Add("\U0001F44F\uFE0F", "Clap", "Body, Hand");
            Add("\U0001F91D\uFE0F", "Shake", "Body, Hand");
            Add("\U0001F64F\uFE0F", "Fold", "Body, Hand");
            Add("\U0000270D\uFE0F", "Write", "Body, Hand");
            Add("\U0000270C\uFE0F", "Victory", "Body, Hand");
            Add("\U0001F91E\uFE0F", "Crossed Fingers", "Body, Hand");
            Add("\U0001F918\uFE0F", "Horns", "Body, Hand");
            Add("\U0001F44C\uFE0F", "OK Hand", "Body, Hand");
            Add("\U0000261D\uFE0F", "Index Up", "Body, Hand");
            Add("\U0001F64C\uFE0F", "Raising Hands", "Body, Hand");
            Add("\U0001F932\uFE0F", "Palms Up", "Body, Hand");
            Add("\U0001FAF6\uFE0F", "Heart Hands", "Body, Hand");

            Add("\U0001F480\uFE0F", "Skull", "Body, Death");
            Add("\U0001F9B4\uFE0F", "Bone", "Body, Death");
            Add("\U00002620\uFE0F", "Skull & Bones", "Body, Death");

            // Smileys
            Add("\U0001F600\uFE0F", "Grinning", "Smiley, Emotion");
            Add("\U0001F606\uFE0F", "Squinting", "Smiley, Emotion");
            Add("\U0001F602\uFE0F", "Laugh", "Smiley, Emotion");
            Add("\U0001F607\uFE0F", "Halo", "Smiley, Emotion");
            Add("\U0001F60D\uFE0F", "Heart Eyes", "Smiley, Emotion");
            Add("\U0001F618\uFE0F", "Kissing", "Smiley, Emotion");
            Add("\U0000263A\uFE0F", "Happy", "Smiley, Emotion");
            Add("\U0001F610\uFE0F", "Neutral", "Smiley, Emotion");
            Add("\U0001F92A\uFE0F", "Zany", "Smiley, Emotion");
            Add("\U0001F911\uFE0F", "Rich", "Smiley, Emotion");
            Add("\U0001FAE3\uFE0F", "Peeking", "Smiley, Emotion");
            Add("\U0001F92B\uFE0F", "Shushing", "Smiley, Emotion");
            Add("\U0001F910\uFE0F", "Zipper Mouth", "Smiley, Emotion");
            Add("\U0001F925\uFE0F", "Lying", "Smiley, Emotion");
            Add("\U0001F60F\uFE0F", "Smirking", "Smiley, Emotion");
            Add("\U0001FAE1\uFE0F", "Saluting", "Smiley, Emotion");
            Add("\U0001F636\uFE0F", "No Mouth", "Smiley, Emotion");
            Add("\U0001FAE0\uFE0F", "Melting", "Smiley, Emotion");
            Add("\U0001F914\uFE0F", "Thinking", "Smiley, Emotion");
            Add("\U0001F644\uFE0F", "Rolling Eyes", "Smiley, Emotion");
            Add("\U0001F62C\uFE0F", "Grimacing", "Smiley, Emotion");
            Add("\U0001F92F\uFE0F", "Mind Blown", "Smiley, Emotion");
            Add("\U0001F634\uFE0F", "Sleeping", "Smiley, Emotion");
            Add("\U0001F971\uFE0F", "Sleepy", "Smiley, Emotion");
            Add("\U0001F915\uFE0F", "Bandage", "Smiley, Emotion");
            Add("\U0001F922\uFE0F", "Nauseated", "Smiley, Emotion");
            Add("\U0001F637\uFE0F", "Mask", "Smiley, Emotion");
            Add("\U0001F912\uFE0F", "Thermometer Face", "Smiley, Emotion");
            Add("\U0001F927\uFE0F", "Sneezing", "Smiley, Emotion");
            Add("\U0001F975\uFE0F", "Hot", "Smiley, Emotion");
            Add("\U0001F976\uFE0F", "Cold", "Smiley, Emotion");
            Add("\U0001F974\uFE0F", "Woozy", "Smiley, Emotion");
            Add("\U0001F635\uFE0F", "Dead", "Smiley, Emotion");
            Add("\U0001F60E\uFE0F", "Cool", "Smiley, Emotion");
            Add("\U0001F913\uFE0F", "Nerd", "Smiley, Emotion");
            Add("\U0001F9D0\uFE0F", "Monocle", "Smiley, Emotion");
            Add("\U0001F978\uFE0F", "Disguised", "Smiley, Emotion");
            Add("\U0001F973\uFE0F", "Party", "Smiley, Emotion");
            Add("\U0001F929\uFE0F", "Star-Struck", "Smiley, Emotion");
            Add("\U00002639\uFE0F", "Frown", "Smiley, Emotion");
            Add("\U0001F61E\uFE0F", "Sad", "Smiley, Emotion");
            Add("\U0001F622\uFE0F", "Crying", "Smiley, Emotion");
            Add("\U0001F97A\uFE0F", "Pleading", "Smiley, Emotion");
            Add("\U0001F979\uFE0F", "Holding Back Tears", "Smiley, Emotion");
            Add("\U0001F628\uFE0F", "Fear", "Smiley, Emotion");
            Add("\U0001F631\uFE0F", "Scream", "Smiley, Emotion");
            Add("\U0001F620\uFE0F", "Angry", "Smiley, Emotion");
            Add("\U0001F624\uFE0F", "Steaming", "Smiley, Emotion");
            Add("\U0001F92C\uFE0F", "Cursing", "Smiley, Emotion");
            Add("\U0001F621\uFE0F", "Enraged Red", "Smiley, Emotion");
            Add("\U0001F4A9\uFE0F", "Poo", "Smiley, Emotion");
            Add("\U0001F648\uFE0F", "See-No-Evil Monkey", "Smiley, Emotion");
            Add("\U0001F649\uFE0F", "Hear-No-Evil Monkey", "Smiley, Emotion");
            Add("\U0001F64A\uFE0F", "Speak-No-Evil Monkey", "Smiley, Emotion");

            // ===== Creatures =====

            // Animal Faces
            Add("\U0001F425\uFE0F", "Baby Chick", "Animal, Face");
            Add("\U0001F427\uFE0F", "Penguin", "Animal, Face");
            Add("\U0001F428\uFE0F", "Koala", "Animal, Face");
            Add("\U0001F42D\uFE0F", "Mouse", "Animal, Face");
            Add("\U0001F42E\uFE0F", "Cow", "Animal, Face");
            Add("\U0001F42F\uFE0F", "Tiger", "Animal, Face");
            Add("\U0001F430\uFE0F", "Rabbit", "Animal, Face");
            Add("\U0001F431\uFE0F", "Cat", "Animal, Face");
            Add("\U0001F434\uFE0F", "Horse", "Animal, Face");
            Add("\U0001F435\uFE0F", "Monkey Face", "Animal, Face");
            Add("\U0001F436\uFE0F", "Dog", "Animal, Face");
            Add("\U0001F437\uFE0F", "Pig", "Animal, Face");
            Add("\U0001F438\uFE0F", "Frog", "Animal, Face");
            Add("\U0001F439\uFE0F", "Hamster", "Animal, Face");
            Add("\U0001F43A\uFE0F", "Wolf", "Animal, Face");
            Add("\U0001F43B\uFE0F", "Bear", "Animal, Face");
            Add("\U0001F43C\uFE0F", "Panda", "Animal, Face");
            Add("\U0001F981\uFE0F", "Lion", "Animal, Face");
            Add("\U0001F98A\uFE0F", "Fox", "Animal, Face");
            Add("\U0001F99D\uFE0F", "Raccoon", "Animal, Face");

            // Animals
            Add("\U0001F426\uFE0F", "Bird", "Animal");
            Add("\U0001F54A\uFE0F", "Dove", "Animal");
            Add("\U0001F985\uFE0F", "Eagle", "Animal");
            Add("\U0001F989\uFE0F", "Owl", "Animal");
            Add("\U0001F986\uFE0F", "Duck", "Animal");
            Add("\U0001F9A2\uFE0F", "Swan", "Animal");
            Add("\U0001FABF\uFE0F", "Goose", "Animal");
            Add("\U0001F99A\uFE0F", "Peacock", "Animal");
            Add("\U0001F99C\uFE0F", "Parrot", "Animal");
            Add("\U0001F9A9\uFE0F", "Flamingo", "Animal");
            Add("\U0001F983\uFE0F", "Turkey", "Animal");
            Add("\U0001F413\uFE0F", "Rooster", "Animal");
            Add("\U0001F424\uFE0F", "Chick", "Animal");
            Add("\U0001F423\uFE0F", "Hatching Chick", "Animal");
            Add("\U0001F414\uFE0F", "Chicken", "Animal");
            Add("\U0001F9A4\uFE0F", "Dodo", "Animal");

            Add("\U0001F400\uFE0F", "Rat", "Animal");
            Add("\U0001F401\uFE0F", "Mouse Body", "Animal");
            Add("\U0001F407\uFE0F", "Rabbit Body", "Animal");
            Add("\U0001F415\uFE0F", "Dog Body", "Animal");
            Add("\U0001F429\uFE0F", "Poodle", "Animal");
            Add("\U0001F408\uFE0F", "Cat Body", "Animal");
            Add("\U0001F43F\uFE0F", "Chipmunk", "Animal");
            Add("\U0001F994\uFE0F", "Hedgehog", "Animal");
            Add("\U0001F9A1\uFE0F", "Badger", "Animal");
            Add("\U0001F9A8\uFE0F", "Skunk", "Animal");
            Add("\U0001F9A6\uFE0F", "Otter", "Animal");
            Add("\U0001F9AB\uFE0F", "Beaver", "Animal");
            Add("\U0001F987\uFE0F", "Bat", "Animal");
            Add("\U0001F9A5\uFE0F", "Sloth", "Animal");
            Add("\U0001F412\uFE0F", "Monkey", "Animal");
            Add("\U0001F98D\uFE0F", "Gorilla", "Animal");
            Add("\U0001F9A7\uFE0F", "Orangutan", "Animal");
            Add("\U0001F998\uFE0F", "Kangaroo", "Animal");

            Add("\U0001F40F\uFE0F", "Ram", "Animal");
            Add("\U0001F410\uFE0F", "Goat", "Animal");
            Add("\U0001F411\uFE0F", "Ewe", "Animal");
            Add("\U0001F417\uFE0F", "Boar", "Animal");
            Add("\U0001F416\uFE0F", "Pig Body", "Animal");
            Add("\U0001F98C\uFE0F", "Deer", "Animal");
            Add("\U0001FACE\uFE0F", "Moose", "Animal");
            Add("\U0001F9AC\uFE0F", "Bison", "Animal");
            Add("\U0001F402\uFE0F", "Ox", "Animal");
            Add("\U0001F403\uFE0F", "Water Buffalo", "Animal");
            Add("\U0001F404\uFE0F", "Cow Body", "Animal");
            Add("\U0001FACF\uFE0F", "Donkey", "Animal");
            Add("\U0001F40E\uFE0F", "Horse Body", "Animal");
            Add("\U0001F999\uFE0F", "Llama", "Animal");
            Add("\U0001F42A\uFE0F", "Camel", "Animal");
            Add("\U0001F42B\uFE0F", "Two-Hump Camel", "Animal");
            Add("\U0001F406\uFE0F", "Leopard", "Animal");
            Add("\U0001F405\uFE0F", "Tiger Body", "Animal");
            Add("\U0001F993\uFE0F", "Zebra", "Animal");
            Add("\U0001F992\uFE0F", "Giraffe", "Animal");
            Add("\U0001F418\uFE0F", "Elephant", "Animal");
            Add("\U0001F98F\uFE0F", "Rhino", "Animal");
            Add("\U0001F99B\uFE0F", "Hippo", "Animal");

            Add("\U0001F40A\uFE0F", "Crocodile", "Animal");
            Add("\U0001F422\uFE0F", "Turtle", "Animal");
            Add("\U0001F98E\uFE0F", "Lizard", "Animal");
            Add("\U0001F40D\uFE0F", "Snake", "Animal");

            Add("\U0001F41C\uFE0F", "Ant", "Animal, Insect");
            Add("\U0001F41D\uFE0F", "Bee", "Animal, Insect");
            Add("\U0001F41E\uFE0F", "Ladybug", "Animal, Insect");
            Add("\U0001FAB2\uFE0F", "Beetle", "Animal, Insect");
            Add("\U0001FAB3\uFE0F", "Cockroach", "Animal, Insect");
            Add("\U0001F997\uFE0F", "Cricket", "Animal, Insect");
            Add("\U0001F98B\uFE0F", "Butterfly", "Animal, Insect");
            Add("\U0001F41B\uFE0F", "Bug", "Animal, Insect");
            Add("\U0001FAB0\uFE0F", "Fly", "Animal, Insect");
            Add("\U0001F99F\uFE0F", "Mosquito", "Animal, Insect");
            Add("\U0001F577\uFE0F", "Spider", "Animal, Insect");
            Add("\U0001F982\uFE0F", "Scorpion", "Animal, Insect");
            Add("\U0001F40C\uFE0F", "Snail", "Animal, Insect");
            Add("\U0001FAB1\uFE0F", "Worm", "Animal, Insect");

            Add("\U0001F995\uFE0F", "Sauropod", "Animal, Extinct");
            Add("\U0001F996\uFE0F", "T-Rex", "Animal, Extinct");
            Add("\U0001F9A3\uFE0F", "Mammoth", "Animal, Extinct");

            Add("\U0001F409\uFE0F", "Dragon", "Animal, Fantasy");
            Add("\U0001F432\uFE0F", "Dragon Face", "Animal, Fantasy");
            Add("\U0001F984\uFE0F", "Unicorn", "Animal, Fantasy");

            // Marine Animals
            Add("\U0001F41F\uFE0F", "Fish", "Animal, Marine");
            Add("\U0001F420\uFE0F", "Tropical Fish", "Animal, Marine");
            Add("\U0001F421\uFE0F", "Blowfish", "Animal, Marine");
            Add("\U0001F988\uFE0F", "Shark", "Animal, Marine");
            Add("\U0001F40B\uFE0F", "Whale", "Animal, Marine");
            Add("\U0001F433\uFE0F", "Spouting Whale", "Animal, Marine");
            Add("\U0001F42C\uFE0F", "Dolphin", "Animal, Marine");
            Add("\U0001F9AD\uFE0F", "Seal", "Animal, Marine");
            Add("\U0001F419\uFE0F", "Octopus", "Animal, Marine");
            Add("\U0001F991\uFE0F", "Squid", "Animal, Marine");
            Add("\U0001FABC\uFE0F", "Jellyfish", "Animal, Marine");
            Add("\U0001F980\uFE0F", "Crab", "Animal, Marine");
            Add("\U0001F99E\uFE0F", "Lobster", "Animal, Marine");
            Add("\U0001F990\uFE0F", "Shrimp", "Animal, Marine");
            Add("\U0001F9AA\uFE0F", "Oyster", "Animal, Marine");
            Add("\U0001F41A\uFE0F", "Shell", "Animal, Marine");
            Add("\U0001FAB8\uFE0F", "Coral", "Animal, Marine");
            Add("\U0001F9A0\uFE0F", "Microbe", "Animal, Marine");

            // Animal Traces
            Add("\U0001F43E\uFE0F", "Pawprints", "Animal, Trace");
            Add("\U0001FAB6\uFE0F", "Feather", "Animal, Trace");
            Add("\U0001FABD\uFE0F", "Wing", "Animal, Trace");
            Add("\U0001F578\uFE0F", "Spiderweb", "Animal, Trace");
            Add("\U0001FABA\uFE0F", "Bird Nest", "Animal, Trace");
            Add("\U0001FAB9\uFE0F", "Empty Nest", "Animal, Trace");

            // ===== Places =====

            // World
            Add("\U0001F30D\uFE0F", "EMEA", "World");
            Add("\U0001F30E\uFE0F", "Americas", "World");
            Add("\U0001F30F\uFE0F", "APAC", "World");
            Add("\U0001F310\uFE0F", "Meridians", "World");
            Add("\U0001F5FA\uFE0F", "Map", "World");

            // Landscapes
            Add("\U000026F0\uFE0F", "Mountain", "World, Landscape");
            Add("\U0001F3D4\uFE0F", "Snowy Mountain", "World, Landscape");
            Add("\U0001F5FB\uFE0F", "Mount Fuji", "World, Landscape");
            Add("\U0001F30B\uFE0F", "Volcano", "World, Landscape");
            Add("\U0001F3DC\uFE0F", "Desert", "World, Landscape");
            Add("\U0001F3D6\uFE0F", "Beach", "World, Landscape");
            Add("\U0001F3DD\uFE0F", "Island", "World, Landscape");
            Add("\U0001F3D5\uFE0F", "Camping", "World, Landscape");
            Add("\U0001F3DE\uFE0F", "National Park", "World, Landscape");
            Add("\U0001F301\uFE0F", "Foggy City", "World, Landscape");
            Add("\U0001F303\uFE0F", "Starry Night", "World, Landscape");
            Add("\U0001F304\uFE0F", "Mountain Sunrise", "World, Landscape");
            Add("\U0001F305\uFE0F", "Sunrise", "World, Landscape");
            Add("\U0001F306\uFE0F", "Dusk", "World, Landscape");
            Add("\U0001F307\uFE0F", "Sunset", "World, Landscape");

            // Structures
            Add("\U0001F3E0\uFE0F", "House", "Structure");
            Add("\U0001F3E1\uFE0F", "House & Garden", "Structure");
            Add("\U0001F3D8\uFE0F", "Houses", "Structure");
            Add("\U0001F3DA\uFE0F", "Derelict House", "Structure");
            Add("\U0001F6D6\uFE0F", "Hut", "Structure");
            Add("\U000026FA\uFE0F", "Tent", "Structure");
            Add("\U0001F3F0\uFE0F", "Castle", "Structure");
            Add("\U0001F3EF\uFE0F", "Japanese Castle", "Structure");
            Add("\U0001F5FC\uFE0F", "Tower", "Structure");
            Add("\U0001F309\uFE0F", "Bridge", "Structure");
            Add("\U0001F6E4\uFE0F", "Railway Track", "Structure");
            Add("\U0001F6E3\uFE0F", "Motorway", "Structure");
            Add("\U0001F3D9\uFE0F", "Cityscape", "Structure");
            Add("\U0001F3D7\uFE0F", "Crane", "Structure");

            Add("\U0001F3DB\uFE0F", "Classical", "Structure");
            Add("\U0001F3DF\uFE0F", "Stadium", "Structure");
            Add("\U0001F3EB\uFE0F", "School", "Structure");
            Add("\U0001F3E5\uFE0F", "Hospital", "Structure");
            Add("\U0001F3E6\uFE0F", "Bank", "Structure");
            Add("\U0001F3E2\uFE0F", "Office", "Structure");
            Add("\U0001F3E4\uFE0F", "Post Office", "Structure");
            Add("\U0001F3E3\uFE0F", "Japanese Post Office", "Structure");
            Add("\U0001F3EA\uFE0F", "Store", "Structure");
            Add("\U0001F3EC\uFE0F", "Department Store", "Structure");
            Add("\U0001F3ED\uFE0F", "Factory", "Structure");
            Add("\U0001F3E8\uFE0F", "Hotel", "Structure");
            Add("\U0001F3E9\uFE0F", "Love Hotel", "Structure");
            Add("\U0001F492\uFE0F", "Wedding", "Structure");

            Add("\U000026F2\uFE0F", "Fountain", "Structure, Decor");
            Add("\U0001F5FD\uFE0F", "Statue", "Structure, Decor");
            Add("\U0001F5FF\uFE0F", "Moai", "Structure, Decor");
            Add("\U0001F488\uFE0F", "Barber Pole", "Structure, Decor");

            // Religion
            Add("\U000026EA\uFE0F", "Church", "Structure, Faith");
            Add("\U0001F54C\uFE0F", "Mosque", "Structure, Faith");
            Add("\U0001F54B\uFE0F", "Kaaba", "Structure, Faith");
            Add("\U0001F54D\uFE0F", "Synagogue", "Structure, Faith");
            Add("\U0001F6D5\uFE0F", "Hindu Temple", "Structure, Faith");
            Add("\U000026E9\uFE0F", "Shinto Shrine", "Structure, Faith");
            Add("\U0001F6D0\uFE0F", "Place of Worship", "Structure, Faith");

            // Leisure
            Add("\U0001F3AA\uFE0F", "Circus Tent", "Structure, Leisure");
            Add("\U0001F3A0\uFE0F", "Carousel Horse", "Structure, Leisure");
            Add("\U0001F3A1\uFE0F", "Ferris Wheel", "Structure, Leisure");
            Add("\U0001F3A2\uFE0F", "Roller Coaster", "Structure, Leisure");
            Add("\U0001F6DD\uFE0F", "Playground Slide", "Structure, Leisure");
            Add("\U000026F1\uFE0F", "Beach Umbrella", "Structure, Leisure");

            // Furniture
            Add("\U0001F6AA\uFE0F", "Door", "Furniture");
            Add("\U0001FA9F\uFE0F", "Window", "Furniture");
            Add("\U0001FA9E\uFE0F", "Mirror", "Furniture");
            Add("\U0001F6CF\uFE0F", "Bed", "Furniture");
            Add("\U0001FA91\uFE0F", "Chair", "Furniture");
            Add("\U0001F6CB\uFE0F", "Couch & Lamp", "Furniture");
            Add("\U0001FA9C\uFE0F", "Ladder", "Furniture");
            Add("\U0001F6D7\uFE0F", "Elevator", "Furniture");
            Add("\U0001F5BC\uFE0F", "Picture", "Furniture");
            Add("\U0001F4EB\uFE0F", "Mailbox", "Furniture");
            Add("\U0001FAA7\uFE0F", "Signboard", "Furniture");
            Add("\U0001FAB4\uFE0F", "Potted Plant", "Furniture");

            // Bathroom
            Add("\U0001F6C1\uFE0F", "Bathtub", "Furniture, Bathroom");
            Add("\U0001F6BF\uFE0F", "Shower", "Furniture, Bathroom");
            Add("\U0001F6BD\uFE0F", "Toilet", "Furniture, Bathroom");
            Add("\U0001F9FB\uFE0F", "Toilet Paper", "Furniture, Bathroom");
            Add("\U0001FAA0\uFE0F", "Plunger", "Furniture, Bathroom");
            Add("\U0001F9FC\uFE0F", "Soap", "Furniture, Bathroom");
            Add("\U0001FAE7\uFE0F", "Bubbles", "Furniture, Bathroom");
            Add("\U0001FAA5\uFE0F", "Toothbrush", "Furniture, Bathroom");
            Add("\U0001FA92\uFE0F", "Razor", "Furniture, Bathroom");
            Add("\U0001F9F4\uFE0F", "Lotion Bottle", "Furniture, Bathroom");

            // ===== Nature =====

            // Sky & Space
            Add("\U00002600\uFE0F", "Sun", "Nature, Sky");
            Add("\U0001F31E\uFE0F", "Sun Face", "Nature, Sky");
            Add("\U0001F311\uFE0F", "Moon", "Nature, Sky");
            Add("\U0001F315\uFE0F", "Full Moon", "Nature, Sky");
            Add("\U0001F31D\uFE0F", "Full Moon Face", "Nature, Sky");
            Add("\U0001F319\uFE0F", "Crescent Moon", "Nature, Sky");
            Add("\U00002B50\uFE0F", "Star", "Nature, Sky");
            Add("\U0001F31F\uFE0F", "Glowing Star", "Nature, Sky");
            Add("\U0001F320\uFE0F", "Shooting Star", "Nature, Sky");
            Add("\U00002601\uFE0F", "Cloud", "Nature, Sky");
            Add("\U00002604\uFE0F", "Comet", "Nature, Space");
            Add("\U0001FA90\uFE0F", "Planet", "Nature, Space");
            Add("\U0001F30C\uFE0F", "Milky Way", "Nature, Space");

            // Elements
            Add("\U0001F525\uFE0F", "Fire", "Nature, Element");
            Add("\U0001F4A7\uFE0F", "Droplet", "Nature, Element");
            Add("\U0001F30A\uFE0F", "Water Wave", "Nature, Element");

            // Weather
            Add("\U000026A1\uFE0F", "Lightning", "Nature, Weather");
            Add("\U00002744\uFE0F", "Snowflake", "Nature, Weather");
            Add("\U0001F32A\uFE0F", "Tornado", "Nature, Weather");
            Add("\U0001F300\uFE0F", "Cyclone", "Nature, Weather");
            Add("\U0001F308\uFE0F", "Rainbow", "Nature, Weather");
            Add("\U000026C5\uFE0F", "Partly Sunny", "Nature, Weather");
            Add("\U0001F327\uFE0F", "Rain Cloud", "Nature, Weather");
            Add("\U000026C8\uFE0F", "Thunderstorm", "Nature, Weather");
            Add("\U0001F328\uFE0F", "Snow Cloud", "Nature, Weather");
            Add("\U0001F32B\uFE0F", "Fog", "Nature, Weather");
            Add("\U0001F32C\uFE0F", "Wind Face", "Nature, Weather");
            Add("\U0001F4A8\uFE0F", "Gust", "Nature, Weather");
            Add("\U00002614\uFE0F", "Rain Umbrella", "Nature, Weather");
            Add("\U00002603\uFE0F", "Snowman", "Nature, Weather");
            Add("\U0001F321\uFE0F", "Thermometer", "Nature, Weather");

            // Plants
            Add("\U0001F331\uFE0F", "Seedling", "Nature, Plant");
            Add("\U0001F33E\uFE0F", "Wheat", "Nature, Plant");
            Add("\U0001F339\uFE0F", "Rose", "Nature, Plant");
            Add("\U0001F33A\uFE0F", "Hibiscus", "Nature, Plant");
            Add("\U0001F940\uFE0F", "Wilted Flower", "Nature, Plant");
            Add("\U0001F33B\uFE0F", "Sunflower", "Nature, Plant");
            Add("\U0001F33C\uFE0F", "Blossom", "Nature, Plant");
            Add("\U0001F338\uFE0F", "Cherry Blossom", "Nature, Plant");
            Add("\U0001F4AE\uFE0F", "White Flower", "Nature, Plant");
            Add("\U0001F3F5\uFE0F", "Rosette", "Nature, Plant");
            Add("\U0001F337\uFE0F", "Tulip", "Nature, Plant");
            Add("\U0001FABB\uFE0F", "Hyacinth", "Nature, Plant");
            Add("\U0001FAB7\uFE0F", "Lotus", "Nature, Plant");
            Add("\U0001F335\uFE0F", "Cactus", "Nature, Plant");

            // Trees
            Add("\U0001F333\uFE0F", "Deciduous Tree", "Nature, Tree");
            Add("\U0001F332\uFE0F", "Evergreen Tree", "Nature, Tree");
            Add("\U0001F334\uFE0F", "Palm Tree", "Nature, Tree");
            Add("\U0001FABE\uFE0F", "Leafless Tree", "Nature, Tree");
            Add("\U0001F38B\uFE0F", "Tanabata Tree", "Nature, Tree");
            Add("\U0001F38D\uFE0F", "Pine Decoration", "Nature, Tree");

            // Leaves
            Add("\U0001F340\uFE0F", "Clover", "Nature, Leaf");
            Add("\U00002618\uFE0F", "Shamrock", "Nature, Leaf");
            Add("\U0001F341\uFE0F", "Maple", "Nature, Leaf");
            Add("\U0001F342\uFE0F", "Fallen Leaf", "Nature, Leaf");
            Add("\U0001F343\uFE0F", "Falling Leaf", "Nature, Leaf");

            // Resources
            Add("\U0001FAA8\uFE0F", "Rock", "Resource");
            Add("\U0001F9F1\uFE0F", "Brick", "Resource");
            Add("\U0001FAB5\uFE0F", "Wood Log", "Resource");
            Add("\U0001F6E2\uFE0F", "Oil Barrel", "Resource");

            // ===== Things =====

            // Weapons
            Add("\U00002694\uFE0F", "Crossed Swords", "Weapon, Melee");
            Add("\U0001F5E1\uFE0F", "Dagger", "Weapon, Melee");
            Add("\U0001FA93\uFE0F", "Axe", "Weapon, Melee");
            Add("\U0001F531\uFE0F", "Trident", "Weapon, Melee");
            Add("\U0001F3F9\uFE0F", "Bow & Arrow", "Weapon, Ranged");
            Add("\U0001FA83\uFE0F", "Boomerang", "Weapon, Ranged");
            Add("\U0001F52B\uFE0F", "Water Pistol", "Weapon, Ranged");
            Add("\U0001F4A3\uFE0F", "Bomb", "Weapon, Explosive");
            Add("\U0001F9E8\uFE0F", "Dynamite", "Weapon, Explosive");
            Add("\U000026D3\uFE0F", "Chains", "Weapon, Trap");
            Add("\U0001FAA4\uFE0F", "Trap", "Weapon, Trap");
            Add("\U0001FA84\uFE0F", "Wand", "Weapon, Magic");
            Add("\U0001F6E1\uFE0F", "Shield", "Weapon, Defense");

            // Items
            Add("\U0001F4DC\uFE0F", "Scroll", "Item, Magic");
            Add("\U0001F52E\uFE0F", "Crystal Ball", "Item, Magic");
            Add("\U0001F9EA\uFE0F", "Potion", "Item, Magic");
            Add("\U00002697\uFE0F", "Alembic", "Item, Magic");

            Add("\U0001F4FF\uFE0F", "Prayer Beads", "Item, Faith");
            Add("\U0001F9FF\uFE0F", "Nazar Amulet", "Item, Faith");
            Add("\U0001FAAC\uFE0F", "Hamsa", "Item, Faith");

            Add("\U0001F4D6\uFE0F", "Open Book", "Item, Knowledge");
            Add("\U0001F4DA\uFE0F", "Books", "Item, Knowledge");
            Add("\U0001F4D5\uFE0F", "Red Book", "Item, Knowledge");
            Add("\U0001F4D7\uFE0F", "Green Book", "Item, Knowledge");
            Add("\U0001F4D8\uFE0F", "Blue Book", "Item, Knowledge");
            Add("\U0001F4D9\uFE0F", "Orange Book", "Item, Knowledge");
            Add("\U0001F4D3\uFE0F", "Notebook", "Item, Knowledge");
            Add("\U0001F4D2\uFE0F", "Ledger", "Item, Knowledge");
            Add("\U0001F516\uFE0F", "Bookmark", "Item, Knowledge");
            Add("\U0001F4F0\uFE0F", "Newspaper", "Item, Knowledge");
            Add("\U0001F5DE\uFE0F", "Rolled Newspaper", "Item, Knowledge");
            Add("\U0001F52D\uFE0F", "Telescope", "Item, Knowledge");
            Add("\U0001F52C\uFE0F", "Microscope", "Item, Knowledge");
            Add("\U0001F9EE\uFE0F", "Abacus", "Item, Knowledge");
            Add("\U0001F9EC\uFE0F", "DNA", "Item, Knowledge");
            Add("\U0001F9EB\uFE0F", "Petri Dish", "Item, Knowledge");
            Add("\U0001F489\uFE0F", "Syringe", "Item, Medicine");
            Add("\U0001FA79\uFE0F", "Adhesive Bandage", "Item, Medicine");
            Add("\U0001FA7A\uFE0F", "Stethoscope", "Item, Medicine");
            Add("\U0001FA7B\uFE0F", "X-Ray", "Item, Medicine");
            Add("\U0001FA7C\uFE0F", "Crutch", "Item, Medicine");
            Add("\U0001F9AF\uFE0F", "White Cane", "Item, Medicine");

            Add("\U0001F528\uFE0F", "Hammer", "Item, Tool");
            Add("\U0001F527\uFE0F", "Wrench", "Item, Tool");
            Add("\U0001F6E0\uFE0F", "Hammer & Wrench", "Item, Tool");
            Add("\U00002692\uFE0F", "Hammer & Pick", "Item, Tool");
            Add("\U0001FA9B\uFE0F", "Screwdriver", "Item, Tool");
            Add("\U0001F529\uFE0F", "Nut & Bolt", "Item, Tool");
            Add("\U0001F5DC\uFE0F", "Clamp", "Item, Tool");
            Add("\U0001FA9D\uFE0F", "Hook", "Item, Tool");
            Add("\U0001FA9A\uFE0F", "Saw", "Item, Tool");
            Add("\U0001FA8F\uFE0F", "Shovel", "Item, Tool");
            Add("\U0001F9F0\uFE0F", "Toolbox", "Item, Tool");
            Add("\U0001F9EF\uFE0F", "Fire Extinguisher", "Item, Tool");
            Add("\U000026CF\uFE0F", "Pick", "Item, Tool");
            Add("\U00002699\uFE0F", "Gear", "Item, Tool");
            Add("\U0001F9ED\uFE0F", "Compass", "Item, Tool");
            Add("\U0001F50D\uFE0F", "Magnifying Glass", "Item, Tool");
            Add("\U0001F9F2\uFE0F", "Magnet", "Item, Tool");
            Add("\U0001F50B\uFE0F", "Battery", "Item, Tool");
            Add("\U0001F9F9\uFE0F", "Broom", "Item, Tool");
            Add("\U0001F9FD\uFE0F", "Sponge", "Item, Tool");
            Add("\U0001F9F5\uFE0F", "Thread", "Item, Craft");
            Add("\U0001F9F6\uFE0F", "Yarn", "Item, Craft");
            Add("\U0001FAA1\uFE0F", "Sewing Needle", "Item, Craft");
            Add("\U0001FAA2\uFE0F", "Knot", "Item, Craft");
            Add("\U0001F9F7\uFE0F", "Safety Pin", "Item, Craft");

            Add("\U0000231B\uFE0F", "Hourglass", "Item, Time");
            Add("\U000023F0\uFE0F", "Alarm Clock", "Item, Time");
            Add("\U000023F3\uFE0F", "Flowing Hourglass", "Item, Time");
            Add("\U000023F1\uFE0F", "Stopwatch", "Item, Time");
            Add("\U0000231A\uFE0F", "Watch", "Item, Time");
            Add("\U0001F570\uFE0F", "Mantelpiece Clock", "Item, Time");
            Add("\U00002696\uFE0F", "Justice Scales", "Item, Weight");

            Add("\U0001F3C6\uFE0F", "Trophy", "Item, Reward");
            Add("\U0001F3C5\uFE0F", "Medal", "Item, Reward");
            Add("\U0001F947\uFE0F", "1st Place", "Item, Reward");
            Add("\U0001F948\uFE0F", "2nd Place", "Item, Reward");
            Add("\U0001F949\uFE0F", "3rd Place", "Item, Reward");
            Add("\U0001F396\uFE0F", "Military Medal", "Item, Reward");
            Add("\U0001F48E\uFE0F", "Gem", "Item, Reward");
            Add("\U0001F48D\uFE0F", "Ring", "Item, Reward");
            Add("\U0001F393\uFE0F", "Graduation Cap", "Item, Reward");
            Add("\U0001F397\uFE0F", "Reminder Ribbon", "Item, Reward");
            Add("\U0001F380\uFE0F", "Red Ribbon", "Item, Reward");
            Add("\U0001F48C\uFE0F", "Love Letter", "Item, Reward");
            Add("\U0001F490\uFE0F", "Flower Bouquet", "Item, Reward");

            Add("\U0001FA99\uFE0F", "Coin", "Item, Economy");
            Add("\U0001F4B0\uFE0F", "Money Bag", "Item, Economy");
            Add("\U0001F4B5\uFE0F", "Banknote", "Item, Economy");
            Add("\U0001F4B3\uFE0F", "Credit Card", "Item, Economy");
            Add("\U0001F9FE\uFE0F", "Receipt", "Item, Economy");
            Add("\U0001F4B8\uFE0F", "Flying Money", "Item, Economy");
            Add("\U0001F4C8\uFE0F", "Chart Up", "Item, Economy");
            Add("\U0001F4C9\uFE0F", "Chart Down", "Item, Economy");

            Add("\U0001F3FA\uFE0F", "Amphora", "Item, Container");
            Add("\U0001FAD9\uFE0F", "Jar", "Item, Container");
            Add("\U0001FAA3\uFE0F", "Bucket", "Item, Container");
            Add("\U0001F9FA\uFE0F", "Basket", "Item, Container");
            Add("\U0001F963\uFE0F", "Bowl", "Item, Container");
            Add("\U0001F961\uFE0F", "Takeout Box", "Item, Container");
            Add("\U0001F6D2\uFE0F", "Shopping Cart", "Item, Container");

            Add("\U0001F56F\uFE0F", "Candle", "Item, Light");
            Add("\U0001F4A1\uFE0F", "Light Bulb", "Item, Light");
            Add("\U0001F526\uFE0F", "Flashlight", "Item, Light");
            Add("\U0001F3EE\uFE0F", "Red Lantern", "Item, Light");
            Add("\U0001FA94\uFE0F", "Diya Lamp", "Item, Light");

            Add("\U00002709\uFE0F", "Envelope", "Item, Office");
            Add("\U0001F4E6\uFE0F", "Package", "Item, Office");
            Add("\U0001F4C5\uFE0F", "Calendar", "Item, Office");
            Add("\U0001F4C1\uFE0F", "File Folder", "Item, Office");
            Add("\U0001F5D2\uFE0F", "Spiral Notepad", "Item, Office");
            Add("\U0001F4C3\uFE0F", "Page", "Item, Office");
            Add("\U0001F4DD\uFE0F", "Memo", "Item, Office");
            Add("\U0000270F\uFE0F", "Pencil", "Item, Office");
            Add("\U00002712\uFE0F", "Nib", "Item, Office");
            Add("\U0001F58B\uFE0F", "Fountain Pen", "Item, Office");
            Add("\U0001F4CF\uFE0F", "Ruler", "Item, Office");
            Add("\U0001F4D0\uFE0F", "Triangular Ruler", "Item, Office");
            Add("\U00002702\uFE0F", "Scissors", "Item, Office");
            Add("\U0001F4CE\uFE0F", "Paperclip", "Item, Office");
            Add("\U0001F3F7\uFE0F", "Label", "Item, Office");
            Add("\U0001F5C4\uFE0F", "File Cabinet", "Item, Office");
            Add("\U0001F5F3\uFE0F", "Ballot Box", "Item, Office");
            Add("\U0001F4EE\uFE0F", "Postbox", "Item, Office");
            Add("\U0001F4CC\uFE0F", "Pushpin", "Item, Office");
            Add("\U0001F4CD\uFE0F", "Round Map Pin", "Item, Office");
            Add("\U0001FAAA\uFE0F", "ID Card", "Item, Office");
            Add("\U0001F5D1\uFE0F", "Wastebasket", "Item, Office");

            Add("\U0001F511\uFE0F", "Key", "Item, Access");
            Add("\U0001F5DD\uFE0F", "Old Key", "Item, Access");
            Add("\U0001F512\uFE0F", "Locked Padlock", "Item, Access");
            Add("\U0001F513\uFE0F", "Open Padlock", "Item, Access");
            Add("\U0001F510\uFE0F", "Locked with Key", "Item, Access");
            Add("\U0001F6A8\uFE0F", "Alarm", "Item, Access");

            Add("\U000026B0\uFE0F", "Coffin", "Item, Death");
            Add("\U0001FAA6\uFE0F", "Headstone", "Item, Death");
            Add("\U000026B1\uFE0F", "Urn", "Item, Death");

            Add("\U0001F381\uFE0F", "Wrapped Gift", "Item, Leisure");
            Add("\U0001F389\uFE0F", "Party Popper", "Item, Leisure");
            Add("\U0001F388\uFE0F", "Balloon", "Item, Leisure");
            Add("\U0001F514\uFE0F", "Bell", "Item, Leisure");
            Add("\U0001F6CE\uFE0F", "Bellhop Bell", "Item, Leisure");
            Add("\U0001F384\uFE0F", "Christmas Tree", "Item, Leisure");
            Add("\U0001F383\uFE0F", "Jack-O-Lantern", "Item, Leisure");
            Add("\U0001F386\uFE0F", "Fireworks", "Item, Leisure");
            Add("\U0001F387\uFE0F", "Sparkler", "Item, Leisure");
            Add("\U0001F38A\uFE0F", "Confetti Ball", "Item, Leisure");
            Add("\U0001F38F\uFE0F", "Carp Streamer", "Item, Leisure");
            Add("\U0001F390\uFE0F", "Wind Chime", "Item, Leisure");
            Add("\U0001F9E7\uFE0F", "Red Envelope", "Item, Leisure");
            Add("\U0001F38E\uFE0F", "Japanese Dolls", "Item, Leisure");
            Add("\U0001FA85\uFE0F", "Pinata", "Item, Leisure");
            Add("\U0001FA86\uFE0F", "Nesting Dolls", "Item, Leisure");
            Add("\U0001F9F8\uFE0F", "Teddy Bear", "Item, Leisure");
            Add("\U0001FAA9\uFE0F", "Mirror Ball", "Item, Leisure");
            Add("\U0001F6AC\uFE0F", "Cigarette", "Item, Leisure");

            // Play
            Add("\U0001F3B2\uFE0F", "Die", "Play, Game");
            Add("\U0000265F\uFE0F", "Chess Pawn", "Play, Game");
            Add("\U0001F0CF\uFE0F", "Joker", "Play, Game");
            Add("\U00002660\uFE0F", "Spades", "Play, Game");
            Add("\U00002665\uFE0F", "Hearts", "Play, Game");
            Add("\U00002666\uFE0F", "Diamonds", "Play, Game");
            Add("\U00002663\uFE0F", "Clubs", "Play, Game");
            Add("\U0001F004\uFE0F", "Mahjong", "Play, Game");
            Add("\U0001F3B4\uFE0F", "Flower Cards", "Play, Game");
            Add("\U0001F3B0\uFE0F", "Slot Machine", "Play, Game");
            Add("\U0001F9E9\uFE0F", "Puzzle", "Play, Game");
            Add("\U0001FA80\uFE0F", "Yo-Yo", "Play, Game");
            Add("\U0001FA81\uFE0F", "Kite", "Play, Game");
            Add("\U0001F3AE\uFE0F", "Video Game", "Play, Game");
            Add("\U0001F3AF\uFE0F", "Bullseye", "Play, Game");
            Add("\U0001F945\uFE0F", "Goal", "Play, Game");
            Add("\U000026BD\uFE0F", "Soccer Ball", "Play, Sport");
            Add("\U0001F3C0\uFE0F", "Basketball", "Play, Sport");
            Add("\U0001F3BE\uFE0F", "Tennis", "Play, Sport");
            Add("\U0001F94A\uFE0F", "Boxing Glove", "Play, Sport");
            Add("\U0001F94B\uFE0F", "Martial Arts Uniform", "Play, Sport");
            Add("\U000026F3\uFE0F", "Golf Flag", "Play, Sport");
            Add("\U0001F3A3\uFE0F", "Fishing Pole", "Play, Sport");
            Add("\U0001F93F\uFE0F", "Diving Mask", "Play, Sport");
            Add("\U0001F6F7\uFE0F", "Sled", "Play, Sport");
            Add("\U000026F8\uFE0F", "Ice Skate", "Play, Sport");
            Add("\U0001F3BF\uFE0F", "Skis", "Play, Sport");

            Add("\U0001F3B5\uFE0F", "Music Note", "Play, Music");
            Add("\U0001F3B6\uFE0F", "Music Notes", "Play, Music");
            Add("\U0001F3BC\uFE0F", "Musical Score", "Play, Music");
            Add("\U0001F3B8\uFE0F", "Guitar", "Play, Music");
            Add("\U0001F941\uFE0F", "Drum", "Play, Music");
            Add("\U0001FA98\uFE0F", "Long Drum", "Play, Music");
            Add("\U0001F3B7\uFE0F", "Saxophone", "Play, Music");
            Add("\U0001F3BA\uFE0F", "Trumpet", "Play, Music");
            Add("\U0001F3BB\uFE0F", "Violin", "Play, Music");
            Add("\U0001FA95\uFE0F", "Banjo", "Play, Music");
            Add("\U0001FA97\uFE0F", "Accordion", "Play, Music");
            Add("\U0001F3B9\uFE0F", "Piano", "Play, Music");
            Add("\U0001FA88\uFE0F", "Flute", "Play, Music");
            Add("\U0001FA87\uFE0F", "Maracas", "Play, Music");
            Add("\U0001FA89\uFE0F", "Harp", "Play, Music");
            Add("\U0001F4EF\uFE0F", "Postal Horn", "Play, Music");

            Add("\U0001F3AD\uFE0F", "Performing Arts", "Play, Art");
            Add("\U0001F3A8\uFE0F", "Artist Palette", "Play, Art");
            Add("\U0001F58C\uFE0F", "Paintbrush", "Play, Art");
            Add("\U0001F58D\uFE0F", "Crayon", "Play, Art");

            // Tech
            Add("\U0001F4BB\uFE0F", "Computer", "Tech");
            Add("\U0001F5A5\uFE0F", "Desktop", "Tech");
            Add("\U00002328\uFE0F", "Keyboard", "Tech");
            Add("\U0001F5A8\uFE0F", "Printer", "Tech");
            Add("\U0001F5B1\uFE0F", "Computer Mouse", "Tech");
            Add("\U0001F4BE\uFE0F", "Floppy Disk", "Tech");
            Add("\U0001F4BF\uFE0F", "Optical Disc", "Tech");
            Add("\U0001F4FC\uFE0F", "Videocassette", "Tech");
            Add("\U0001F579\uFE0F", "Joystick", "Tech");
            Add("\U0001F4F1\uFE0F", "Mobile Phone", "Tech");
            Add("\U0000260E\uFE0F", "Telephone", "Tech");
            Add("\U0001F4FA\uFE0F", "TV", "Tech");
            Add("\U0001F4FB\uFE0F", "Radio", "Tech");
            Add("\U0001F4E1\uFE0F", "Antenna", "Tech");
            Add("\U0001F50C\uFE0F", "Plug", "Tech");
            Add("\U0001F4F7\uFE0F", "Camera", "Tech");
            Add("\U0001F4F9\uFE0F", "Video Camera", "Tech");
            Add("\U0001F3A5\uFE0F", "Movie Camera", "Tech");
            Add("\U0001F4FD\uFE0F", "Film Projector", "Tech");
            Add("\U0001F39E\uFE0F", "Film Frames", "Tech");
            Add("\U0001F3AC\uFE0F", "Clapperboard", "Tech");
            Add("\U0001F3A4\uFE0F", "Microphone", "Tech");
            Add("\U0001F399\uFE0F", "Studio Microphone", "Tech");
            Add("\U0001F3A7\uFE0F", "Headphone", "Tech");
            Add("\U0001F4E2\uFE0F", "Loudspeaker", "Tech");
            Add("\U0001F4E3\uFE0F", "Megaphone", "Tech");
            Add("\U0001F50A\uFE0F", "Speaker", "Tech");
            Add("\U0001F507\uFE0F", "Muted Speaker", "Tech");

            // ===== Food & Drink =====

            // Fruits
            Add("\U0001F34E\uFE0F", "Apple", "Food, Fruit");
            Add("\U0001F34F\uFE0F", "Green Apple", "Food, Fruit");
            Add("\U0001F350\uFE0F", "Pear", "Food, Fruit");
            Add("\U0001F351\uFE0F", "Peach", "Food, Fruit");
            Add("\U0001F352\uFE0F", "Cherry", "Food, Fruit");
            Add("\U0001F353\uFE0F", "Strawberry", "Food, Fruit");
            Add("\U0001FAD0\uFE0F", "Berries", "Food, Fruit");
            Add("\U0001F347\uFE0F", "Grapes", "Food, Fruit");
            Add("\U0001F34A\uFE0F", "Tangerine", "Food, Fruit");
            Add("\U0001F34B\uFE0F", "Lemon", "Food, Fruit");
            Add("\U0001F34C\uFE0F", "Banana", "Food, Fruit");
            Add("\U0001F34D\uFE0F", "Pineapple", "Food, Fruit");
            Add("\U0001F96D\uFE0F", "Mango", "Food, Fruit");
            Add("\U0001F95D\uFE0F", "Kiwi", "Food, Fruit");
            Add("\U0001F965\uFE0F", "Coconut", "Food, Fruit");
            Add("\U0001F348\uFE0F", "Melon", "Food, Fruit");
            Add("\U0001F349\uFE0F", "Watermelon", "Food, Fruit");
            Add("\U0001F345\uFE0F", "Tomato", "Food, Fruit");
            Add("\U0001FAD2\uFE0F", "Olive", "Food, Fruit");

            // Veggies
            Add("\U0001F955\uFE0F", "Carrot", "Food, Veggie");
            Add("\U0001F954\uFE0F", "Potato", "Food, Veggie");
            Add("\U0001F360\uFE0F", "Sweet Potato", "Food, Veggie");
            Add("\U0001F33D\uFE0F", "Corn", "Food, Veggie");
            Add("\U0001F966\uFE0F", "Broccoli", "Food, Veggie");
            Add("\U0001F96C\uFE0F", "Lettuce", "Food, Veggie");
            Add("\U0001F952\uFE0F", "Cucumber", "Food, Veggie");
            Add("\U0001F346\uFE0F", "Eggplant", "Food, Veggie");
            Add("\U0001F951\uFE0F", "Avocado", "Food, Veggie");
            Add("\U0001F336\uFE0F", "Pepper", "Food, Veggie");
            Add("\U0001FAD1\uFE0F", "Bell Pepper", "Food, Veggie");
            Add("\U0001F9C5\uFE0F", "Onion", "Food, Veggie");
            Add("\U0001F9C4\uFE0F", "Garlic", "Food, Veggie");
            Add("\U0001FADA\uFE0F", "Ginger", "Food, Veggie");
            Add("\U0001FADC\uFE0F", "Radish", "Food, Veggie");
            Add("\U0001FADB\uFE0F", "Peas", "Food, Veggie");
            Add("\U0001FAD8\uFE0F", "Beans", "Food, Veggie");
            Add("\U0001F95C\uFE0F", "Peanut", "Food, Veggie");
            Add("\U0001F330\uFE0F", "Chestnut", "Food, Veggie");
            Add("\U0001F344\uFE0F", "Fly Agaric", "Food, Veggie");
            Add("\U0001F33F\uFE0F", "Herbs", "Food, Veggie");

            // Food
            Add("\U0001F35E\uFE0F", "Bread", "Food");
            Add("\U0001F950\uFE0F", "Croissant", "Food");
            Add("\U0001F956\uFE0F", "Baguette", "Food");
            Add("\U0001F968\uFE0F", "Pretzel", "Food");
            Add("\U0001F96F\uFE0F", "Bagel", "Food");
            Add("\U0001FAD3\uFE0F", "Pita", "Food");

            Add("\U0001F95A\uFE0F", "Egg", "Food");
            Add("\U0001F9C0\uFE0F", "Cheese", "Food");
            Add("\U0001F9C8\uFE0F", "Butter", "Food");
            Add("\U0001F9C2\uFE0F", "Salt", "Food");
            Add("\U0001F96B\uFE0F", "Can", "Food");

            Add("\U0001F356\uFE0F", "Meat", "Food, Meat");
            Add("\U0001F357\uFE0F", "Poultry", "Food, Meat");
            Add("\U0001F969\uFE0F", "Steak", "Food, Meat");
            Add("\U0001F953\uFE0F", "Bacon", "Food, Meat");

            Add("\U0001F372\uFE0F", "Stew", "Food");
            Add("\U0001F958\uFE0F", "Shallow Pan", "Food");
            Add("\U0001FAD5\uFE0F", "Fondue", "Food");
            Add("\U0001F957\uFE0F", "Salad", "Food");
            Add("\U0001F35C\uFE0F", "Noodle Soup", "Food");
            Add("\U0001F35D\uFE0F", "Spaghetti", "Food");
            Add("\U0001F35A\uFE0F", "Cooked Rice", "Food");
            Add("\U0001F359\uFE0F", "Rice Ball", "Food");
            Add("\U0001F358\uFE0F", "Rice Cracker", "Food");
            Add("\U0001F35B\uFE0F", "Curry Rice", "Food");
            Add("\U0001F371\uFE0F", "Bento Box", "Food");
            Add("\U0001F363\uFE0F", "Sushi", "Food");
            Add("\U0001F364\uFE0F", "Fried Shrimp", "Food");
            Add("\U0001F95F\uFE0F", "Dumpling", "Food");
            Add("\U0001F9C6\uFE0F", "Falafel", "Food");
            Add("\U0001F959\uFE0F", "Stuffed Flatbread", "Food");
            Add("\U0001FAD4\uFE0F", "Tamale", "Food");
            Add("\U0001F355\uFE0F", "Pizza", "Food");
            Add("\U0001F354\uFE0F", "Burger", "Food");
            Add("\U0001F32D\uFE0F", "Hot Dog", "Food");
            Add("\U0001F96A\uFE0F", "Sandwich", "Food");
            Add("\U0001F32E\uFE0F", "Taco", "Food");
            Add("\U0001F32F\uFE0F", "Burrito", "Food");
            Add("\U0001F35F\uFE0F", "Fries", "Food");

            Add("\U0001F382\uFE0F", "Cake", "Food, Sweet");
            Add("\U0001F370\uFE0F", "Shortcake", "Food, Sweet");
            Add("\U0001F9C1\uFE0F", "Cupcake", "Food, Sweet");
            Add("\U0001F967\uFE0F", "Pie", "Food, Sweet");
            Add("\U0001F96E\uFE0F", "Moon Cake", "Food, Sweet");
            Add("\U0001F960\uFE0F", "Fortune Cookie", "Food, Sweet");
            Add("\U0001F361\uFE0F", "Dango", "Food, Sweet");
            Add("\U0001F95E\uFE0F", "Pancakes", "Food, Sweet");
            Add("\U0001F9C7\uFE0F", "Waffle", "Food, Sweet");
            Add("\U0001F369\uFE0F", "Doughnut", "Food, Sweet");
            Add("\U0001F36A\uFE0F", "Cookie", "Food, Sweet");
            Add("\U0001F36B\uFE0F", "Chocolate", "Food, Sweet");
            Add("\U0001F36C\uFE0F", "Candy", "Food, Sweet");
            Add("\U0001F36D\uFE0F", "Lollipop", "Food, Sweet");
            Add("\U0001F36E\uFE0F", "Custard", "Food, Sweet");
            Add("\U0001F36F\uFE0F", "Honey", "Food, Sweet");
            Add("\U0001F37F\uFE0F", "Popcorn", "Food, Sweet");
            Add("\U0001F366\uFE0F", "Soft Ice", "Food, Sweet");
            Add("\U0001F368\uFE0F", "Ice Cream", "Food, Sweet");
            Add("\U0001F367\uFE0F", "Shaved Ice", "Food, Sweet");

            Add("\U0001F48A\uFE0F", "Pill", "Food, Medicine");

            // Eat
            Add("\U0001F373\uFE0F", "Cooking", "Eat");
            Add("\U0001F37D\uFE0F", "Fork, Knife, Plate", "Eat, Cutlery");
            Add("\U0001F374\uFE0F", "Fork & Knife", "Eat, Cutlery");
            Add("\U0001F52A\uFE0F", "Knife", "Eat, Cutlery");
            Add("\U0001F944\uFE0F", "Spoon", "Eat, Cutlery");
            Add("\U0001F962\uFE0F", "Chopsticks", "Eat, Cutlery");

            // Drink
            Add("\U00002615\uFE0F", "Cup", "Drink");
            Add("\U0001F375\uFE0F", "Tea", "Drink");
            Add("\U0001FAD6\uFE0F", "Tea Pot", "Drink");
            Add("\U0001F9C9\uFE0F", "Mate", "Drink");
            Add("\U0001F95B\uFE0F", "Glas", "Drink");
            Add("\U0001F37C\uFE0F", "Baby Bottle", "Drink");
            Add("\U0001F964\uFE0F", "Cup With Straw", "Drink");
            Add("\U0001F9C3\uFE0F", "Juice Box", "Drink");
            Add("\U0001F9CB\uFE0F", "Bubble Tea", "Drink");
            Add("\U0001F9CA\uFE0F", "Ice Cube", "Drink");
            Add("\U0001F37A\uFE0F", "Beer", "Drink");
            Add("\U0001F37B\uFE0F", "Clinking Beers", "Drink");
            Add("\U0001F377\uFE0F", "Wine", "Drink");
            Add("\U0001F37E\uFE0F", "Champagne", "Drink");
            Add("\U0001F942\uFE0F", "Clinking Glasses", "Drink");
            Add("\U0001F376\uFE0F", "Sake", "Drink");
            Add("\U0001F943\uFE0F", "Tumbler", "Drink");
            Add("\U0001FAD7\uFE0F", "Pouring Liquid", "Drink");
            Add("\U0001F378\uFE0F", "Cocktail", "Drink");
            Add("\U0001F379\uFE0F", "Tropical", "Drink");

            // ===== Mobility =====

            // Land
            Add("\U0001F697\uFE0F", "Car", "Mobility, Land");
            Add("\U0001F698\uFE0F", "Oncoming Car", "Mobility, Land");
            Add("\U0001F699\uFE0F", "Sport Utility Vehicle", "Mobility, Land");
            Add("\U0001F3CE\uFE0F", "Racing Car", "Mobility, Land");
            Add("\U0001F695\uFE0F", "Taxi", "Mobility, Land");
            Add("\U0001F696\uFE0F", "Oncoming Taxi", "Mobility, Land");
            Add("\U0001F693\uFE0F", "Police Car", "Mobility, Land");
            Add("\U0001F694\uFE0F", "Oncoming Police", "Mobility, Land");
            Add("\U0001F691\uFE0F", "Ambulance", "Mobility, Land");
            Add("\U0001F692\uFE0F", "Fire Truck", "Mobility, Land");
            Add("\U0001F6FB\uFE0F", "Pickup Truck", "Mobility, Land");
            Add("\U0001F69A\uFE0F", "Delivery Truck", "Mobility, Land");
            Add("\U0001F69B\uFE0F", "Articulated Lorry", "Mobility, Land");
            Add("\U0001F69C\uFE0F", "Tractor", "Mobility, Land");

            Add("\U0001F68C\uFE0F", "Bus", "Mobility, Land");
            Add("\U0001F68D\uFE0F", "Oncoming Bus", "Mobility, Land");
            Add("\U0001F690\uFE0F", "Minibus", "Mobility, Land");
            Add("\U0001F68E\uFE0F", "Trolleybus", "Mobility, Land");
            Add("\U0001F68F\uFE0F", "Bus Stop", "Mobility, Land");

            Add("\U0001F682\uFE0F", "Locomotive", "Mobility, Land");
            Add("\U0001F683\uFE0F", "Railway Car", "Mobility, Land");
            Add("\U0001F686\uFE0F", "Train", "Mobility, Land");
            Add("\U0001F684\uFE0F", "High-Speed Train", "Mobility, Land");
            Add("\U0001F685\uFE0F", "Bullet Train", "Mobility, Land");
            Add("\U0001F687\uFE0F", "Metro", "Mobility, Land");
            Add("\U0001F688\uFE0F", "Light Rail", "Mobility, Land");
            Add("\U0001F69D\uFE0F", "Monorail", "Mobility, Land");
            Add("\U0001F68A\uFE0F", "Tram", "Mobility, Land");
            Add("\U0001F68B\uFE0F", "Tram Car", "Mobility, Land");
            Add("\U0001F689\uFE0F", "Station", "Mobility, Land");
            Add("\U0001F69E\uFE0F", "Mountain Railway", "Mobility, Land");
            Add("\U0001F69F\uFE0F", "Suspension Railway", "Mobility, Land");
            Add("\U0001F6A0\uFE0F", "Mountain Cableway", "Mobility, Land");
            Add("\U0001F6A1\uFE0F", "Aerial Tramway", "Mobility, Land");

            Add("\U0001F6B2\uFE0F", "Bicycle", "Mobility, Land");
            Add("\U0001F3CD\uFE0F", "Motorcycle", "Mobility, Land");
            Add("\U0001F6F5\uFE0F", "Motor Scooter", "Mobility, Land");
            Add("\U0001F6F4\uFE0F", "Kick Scooter", "Mobility, Land");
            Add("\U0001F6F9\uFE0F", "Skateboard", "Mobility, Land");
            Add("\U0001F6FC\uFE0F", "Roller Skate", "Mobility, Land");
            Add("\U0001F6FA\uFE0F", "Auto Rickshaw", "Mobility, Land");
            Add("\U0001F9BD\uFE0F", "Manual Wheelchair", "Mobility, Land");
            Add("\U0001F9BC\uFE0F", "Motorized Wheelchair", "Mobility, Land");

            Add("\U0001F6DB\uFE0F", "Wheel", "Mobility, Land");
            Add("\U000026FD\uFE0F", "Fuel Pump", "Mobility, Land");
            Add("\U0001F6A5\uFE0F", "Horizontal Traffic Light", "Mobility, Land");
            Add("\U0001F6A6\uFE0F", "Vertical Traffic Light", "Mobility, Land");

            // Water
            Add("\U000026F5\uFE0F", "Sailboat", "Mobility, Water");
            Add("\U0001F6F6\uFE0F", "Canoe", "Mobility, Water");
            Add("\U0001F6A4\uFE0F", "Speedboat", "Mobility, Water");
            Add("\U0001F6E5\uFE0F", "Motor Boat", "Mobility, Water");
            Add("\U000026F4\uFE0F", "Ferry", "Mobility, Water");
            Add("\U0001F6F3\uFE0F", "Passenger Ship", "Mobility, Water");
            Add("\U0001F6A2\uFE0F", "Ship", "Mobility, Water");
            Add("\U00002693\uFE0F", "Anchor", "Mobility, Water");
            Add("\U0001F6DF\uFE0F", "Ring Buoy", "Mobility, Water");

            // Air
            Add("\U00002708\uFE0F", "Airplane", "Mobility, Air");
            Add("\U0001F6E9\uFE0F", "Small Airplane", "Mobility, Air");
            Add("\U0001F6EB\uFE0F", "Departure", "Mobility, Air");
            Add("\U0001F6EC\uFE0F", "Arrival", "Mobility, Air");
            Add("\U0001FA82\uFE0F", "Parachute", "Mobility, Air");
            Add("\U0001F4BA\uFE0F", "Seat", "Mobility, Air");
            Add("\U0001F681\uFE0F", "Helicopter", "Mobility, Air");
            Add("\U0001F680\uFE0F", "Rocket", "Mobility, Air");
            Add("\U0001F6F0\uFE0F", "Satellite", "Mobility, Air");
            Add("\U0001F6F8\uFE0F", "UFO", "Mobility, Air");

            // ===== Symbols =====

            // Signs
            Add("\U0001F6D1\uFE0F", "Stop Sign", "Symbol, Sign");
            Add("\U0001F6A7\uFE0F", "Construction Sign", "Symbol, Sign");
            Add("\U00002668\uFE0F", "Hot Springs", "Symbol, Sign");
            Add("\U0001F6BB\uFE0F", "Restroom", "Symbol, Sign");
            Add("\U0001F6B9\uFE0F", "Men's Room", "Symbol, Sign");
            Add("\U0001F6BA\uFE0F", "Women's Room", "Symbol, Sign");
            Add("\U0001F6BC\uFE0F", "Baby Symbol", "Symbol, Sign");
            Add("\U0001F6BE\uFE0F", "Water Closet", "Symbol, Sign");
            Add("\U0000267F\uFE0F", "Wheelchair Symbol", "Symbol, Sign");
            Add("\U0001F6B0\uFE0F", "Potable Water", "Symbol, Sign");
            Add("\U0001F6B8\uFE0F", "Children Crossing", "Symbol, Sign");
            Add("\U0001F6AD\uFE0F", "No Smoking", "Symbol, Sign");
            Add("\U0001F51E\uFE0F", "No One Under 18", "Symbol, Sign");

            // Status
            Add("\U00002705\uFE0F", "Check", "Symbol, Status");
            Add("\U00002714\uFE0F", "Check Mark", "Symbol, Status");
            Add("\U0000274C\uFE0F", "Cross Mark", "Symbol, Status");
            Add("\U00002716\uFE0F", "Multiply", "Symbol, Status");
            Add("\U00002B55\uFE0F", "Hollow Circle", "Symbol, Status");
            Add("\U00002753\uFE0F", "Question", "Symbol, Status");
            Add("\U00002757\uFE0F", "Exclamation", "Symbol, Status");
            Add("\U0000203C\uFE0F", "Double Exclamation", "Symbol, Status");
            Add("\U00002049\uFE0F", "Exclamation Question", "Symbol, Status");
            Add("\U000026A0\uFE0F", "Warning", "Symbol, Status");
            Add("\U000026D4\uFE0F", "No Entry", "Symbol, Status");
            Add("\U0001F6AB\uFE0F", "Prohibited", "Symbol, Status");
            Add("\U00002622\uFE0F", "Radioactive", "Symbol, Status");
            Add("\U00002623\uFE0F", "Biohazard", "Symbol, Status");
            Add("\U00002695\uFE0F", "Medical", "Symbol, Status");
            Add("\U00002139\uFE0F", "Information", "Symbol, Status");
            Add("\U0001F198\uFE0F", "SOS", "Symbol, Status");
            Add("\U0001F195\uFE0F", "New", "Symbol, Status");

            // Markers
            Add("\U0001F534\uFE0F", "Red Circle", "Symbol, Marker");
            Add("\U0001F7E0\uFE0F", "Orange Circle", "Symbol, Marker");
            Add("\U0001F7E1\uFE0F", "Yellow Circle", "Symbol, Marker");
            Add("\U0001F7E2\uFE0F", "Green Circle", "Symbol, Marker");
            Add("\U0001F535\uFE0F", "Blue Circle", "Symbol, Marker");
            Add("\U0001F7E3\uFE0F", "Purple Circle", "Symbol, Marker");
            Add("\U0001F7E4\uFE0F", "Brown Circle", "Symbol, Marker");
            Add("\U000026AB\uFE0F", "Black Circle", "Symbol, Marker");
            Add("\U000026AA\uFE0F", "White Circle", "Symbol, Marker");
            Add("\U0001F7E5\uFE0F", "Red Square", "Symbol, Marker");
            Add("\U0001F7E7\uFE0F", "Orange Square", "Symbol, Marker");
            Add("\U0001F7E8\uFE0F", "Yellow Square", "Symbol, Marker");
            Add("\U0001F7E9\uFE0F", "Green Square", "Symbol, Marker");
            Add("\U0001F7E6\uFE0F", "Blue Square", "Symbol, Marker");
            Add("\U0001F7EA\uFE0F", "Purple Square", "Symbol, Marker");
            Add("\U0001F7EB\uFE0F", "Brown Square", "Symbol, Marker");
            Add("\U00002B1B\uFE0F", "Black Square", "Symbol, Marker");
            Add("\U00002B1C\uFE0F", "White Square", "Symbol, Marker");
            Add("\U0001F53A\uFE0F", "Red Triangle Up", "Symbol, Marker");
            Add("\U0001F53B\uFE0F", "Red Triangle Down", "Symbol, Marker");
            Add("\U0001F536\uFE0F", "Orange Diamond", "Symbol, Marker");
            Add("\U0001F537\uFE0F", "Blue Diamond", "Symbol, Marker");

            // Expressions
            Add("\U0001F4AC\uFE0F", "Speech Bubble", "Symbol");
            Add("\U0001F5E8\uFE0F", "Left Speech Bubble", "Symbol");
            Add("\U0001F4AD\uFE0F", "Thought Bubble", "Symbol");
            Add("\U0001F5EF\uFE0F", "Anger Bubble", "Symbol");
            Add("\U0001F4A4\uFE0F", "ZZZ", "Symbol");
            Add("\U0001F4AF\uFE0F", "100", "Symbol");
            Add("\U00002764\uFE0F", "Heart", "Symbol");
            Add("\U0001FA77\uFE0F", "Pink Heart", "Symbol");
            Add("\U0001F9E1\uFE0F", "Orange Heart", "Symbol");
            Add("\U0001F49B\uFE0F", "Yellow Heart", "Symbol");
            Add("\U0001F49A\uFE0F", "Green Heart", "Symbol");
            Add("\U0001FA75\uFE0F", "Light Blue Heart", "Symbol");
            Add("\U0001F499\uFE0F", "Blue Heart", "Symbol");
            Add("\U0001F49C\uFE0F", "Purple Heart", "Symbol");
            Add("\U0001F90E\uFE0F", "Brown Heart", "Symbol");
            Add("\U0001F5A4\uFE0F", "Black Heart", "Symbol");
            Add("\U0001FA76\uFE0F", "Grey Heart", "Symbol");
            Add("\U0001F90D\uFE0F", "White Heart", "Symbol");
            Add("\U00002763\uFE0F", "Heart Exclamation", "Symbol");
            Add("\U0001F495\uFE0F", "Two Hearts", "Symbol");
            Add("\U0001F494\uFE0F", "Broken Heart", "Symbol");
            Add("\U0001F48B\uFE0F", "Kiss", "Symbol");
            Add("\U00002728\uFE0F", "Sparkles", "Symbol");
            Add("\U0001F4AB\uFE0F", "Dizzy", "Symbol");
            Add("\U0001F4A5\uFE0F", "Collision", "Symbol");
            Add("\U0001F4A2\uFE0F", "Anger Symbol", "Symbol");
            Add("\U0001F573\uFE0F", "Hole", "Symbol");
            Add("\U0001F4A6\uFE0F", "Droplets", "Symbol");
            Add("\U0001FADF\uFE0F", "Splatter", "Symbol");
            Add("\U0000267E\uFE0F", "Infinity", "Symbol");
            Add("\U0001F517\uFE0F", "Link", "Symbol");
            Add("\U0000267B\uFE0F", "Recycling Symbol", "Symbol");
            Add("\U0000269B\uFE0F", "Atom", "Symbol");

            // Faith
            Add("\U0000271D\uFE0F", "Latin Cross", "Symbol, Faith");
            Add("\U00002626\uFE0F", "Orthodox Cross", "Symbol, Faith");
            Add("\U0000262A\uFE0F", "Star & Crescent", "Symbol, Faith");
            Add("\U00002721\uFE0F", "Star of David", "Symbol, Faith");
            Add("\U0001F52F\uFE0F", "Dotted Six-Pointed Star", "Symbol, Faith");
            Add("\U0001F549\uFE0F", "Om", "Symbol, Faith");
            Add("\U0001FAAF\uFE0F", "Khanda", "Symbol, Faith");
            Add("\U00002638\uFE0F", "Dharma Wheel", "Symbol, Faith");
            Add("\U0000262F\uFE0F", "Yin Yang", "Symbol, Faith");
            Add("\U0000262E\uFE0F", "Peace", "Symbol, Faith");

            // Zodiac
            Add("\U00002648\uFE0F", "Aries", "Symbol, Zodiac");
            Add("\U00002649\uFE0F", "Taurus", "Symbol, Zodiac");
            Add("\U0000264A\uFE0F", "Gemini", "Symbol, Zodiac");
            Add("\U0000264B\uFE0F", "Cancer", "Symbol, Zodiac");
            Add("\U0000264C\uFE0F", "Leo", "Symbol, Zodiac");
            Add("\U0000264D\uFE0F", "Virgo", "Symbol, Zodiac");
            Add("\U0000264E\uFE0F", "Libra", "Symbol, Zodiac");
            Add("\U0000264F\uFE0F", "Scorpio", "Symbol, Zodiac");
            Add("\U00002650\uFE0F", "Sagittarius", "Symbol, Zodiac");
            Add("\U00002651\uFE0F", "Capricorn", "Symbol, Zodiac");
            Add("\U00002652\uFE0F", "Aquarius", "Symbol, Zodiac");
            Add("\U00002653\uFE0F", "Pisces", "Symbol, Zodiac");
            Add("\U000026CE\uFE0F", "Ophiuchus", "Symbol, Zodiac");

            // Gender
            Add("\U00002640\uFE0F", "Female", "Symbol, Gender");
            Add("\U00002642\uFE0F", "Male", "Symbol, Gender");
            Add("\U000026A7\uFE0F", "Transgender", "Symbol, Gender");

            // Letters & Buttons
            Add("\U0001F524\uFE0F", "Letters (ABC)", "Symbol, Sign");
            Add("\U0001F522\uFE0F", "Numbers (123)", "Symbol, Sign");
            Add("\U000025B6\uFE0F", "Play Button", "Symbol, Sign");
            Add("\U000023F8\uFE0F", "Pause Button", "Symbol, Sign");
            Add("\U000023F9\uFE0F", "Stop Button", "Symbol, Sign");
            Add("\U000023E9\uFE0F", "Fast-Forward Button", "Symbol, Sign");
            Add("\U000023EA\uFE0F", "Fast Reverse Button", "Symbol, Sign");
            Add("\U000023ED\uFE0F", "Next Track Button", "Symbol, Sign");
            Add("\U000023EE\uFE0F", "Last Track Button", "Symbol, Sign");
            Add("\U0001F501\uFE0F", "Repeat Button", "Symbol, Sign");
            Add("\U0001F504\uFE0F", "Arrows Circle", "Symbol, Sign");
            Add("\U00002B06\uFE0F", "Up Arrow", "Symbol, Sign");
            Add("\U000027A1\uFE0F", "Right Arrow", "Symbol, Sign");
            Add("\U00002B07\uFE0F", "Down Arrow", "Symbol, Sign");
            Add("\U00002B05\uFE0F", "Left Arrow", "Symbol, Sign");
            Add("\U00002795\uFE0F", "Plus", "Symbol, Sign");
            Add("\U00002796\uFE0F", "Minus", "Symbol, Sign");

            // Flags & Heraldry
            Add("\U0001F6A9\uFE0F", "Triangular Flag", "Symbol, Flag");
            Add("\U0001F3F3\uFE0F", "White Flag", "Symbol, Flag");
            Add("\U0001F3F4\uFE0F", "Black Flag", "Symbol, Flag");
            Add("\U0001F3C1\uFE0F", "Chequered Flag", "Symbol, Flag");
            Add("\U0001F38C\uFE0F", "Crossed Flags", "Symbol, Flag");
            Add("\U0000269C\uFE0F", "Fleur-de-lis", "Symbol, Heraldry");
        }
#endif
        #endregion
    }
}
