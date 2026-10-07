using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class GlyphLibraryCollection : ScriptableObject
    {
        #region Fields
        [SerializeField, HideInInspector]
        private List<GlyphDefinition> _glyphs = new();
        #endregion

        #region Properties
        public IReadOnlyList<GlyphDefinition> Glyphs => _glyphs;
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
            Add("\U0001F462\uFE0F", "Boot", "Clothes");

            Add("\U0001F452\uFE0F", "Hat", "Clothes, Headwear");
            Add("\U0001F3A9\uFE0F", "Top Hat", "Clothes, Headwear");
            Add("\U000026D1\uFE0F", "Rescue Helmet", "Clothes, Headwear");
            Add("\U0001FA96\uFE0F", "Military Helmet", "Clothes, Headwear");
            Add("\U0001F451\uFE0F", "Crown", "Clothes, Fantasy");

            Add("\U0001F453\uFE0F", "Glasses", "Clothes, Accessory");
            Add("\U0001F392\uFE0F", "Backpack", "Clothes, Accessory");
            Add("\U0001F9F3\uFE0F", "Luggage", "Clothes, Accessory");

            // Body
            Add("\U0001F441\uFE0F", "Eye", "Body");
            Add("\U0001F442\uFE0F", "Ear", "Body");
            Add("\U0001F443\uFE0F", "Nose", "Body");
            Add("\U0001F444\uFE0F", "Mouth", "Body");
            Add("\U0001F4AA\uFE0F", "Arm", "Body");
            Add("\U0001F9B5\uFE0F", "Leg", "Body");
            Add("\U0001F9B6\uFE0F", "Foot", "Body");
            Add("\U0001F463\uFE0F", "Footprints", "Body");
            Add("\U0001FAC6\uFE0F", "Fingerprint", "Body");

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
            Add("\U0001F914\uFE0F", "Thinking", "Smiley, Emotion");
            Add("\U0001F644\uFE0F", "Rolling Eyes", "Smiley, Emotion");
            Add("\U0001F62C\uFE0F", "Grimacing", "Smiley, Emotion");
            Add("\U0001F92F\uFE0F", "Mind Blown", "Smiley, Emotion");
            Add("\U0001F634\uFE0F", "Sleeping", "Smiley, Emotion");
            Add("\U0001F971\uFE0F", "Sleepy", "Smiley, Emotion");
            Add("\U0001F915\uFE0F", "Bandage", "Smiley, Emotion");
            Add("\U0001F922\uFE0F", "Nauseated", "Smiley, Emotion");
            Add("\U0001F975\uFE0F", "Hot", "Smiley, Emotion");
            Add("\U0001F976\uFE0F", "Cold", "Smiley, Emotion");
            Add("\U0001F974\uFE0F", "Woozy", "Smiley, Emotion");
            Add("\U0001F635\uFE0F", "Dead", "Smiley, Emotion");
            Add("\U0001F60E\uFE0F", "Cool", "Smiley, Emotion");
            Add("\U00002639\uFE0F", "Frown", "Smiley, Emotion");
            Add("\U0001F61E\uFE0F", "Sad", "Smiley, Emotion");
            Add("\U0001F622\uFE0F", "Crying", "Smiley, Emotion");
            Add("\U0001F628\uFE0F", "Fear", "Smiley, Emotion");
            Add("\U0001F631\uFE0F", "Scream", "Smiley, Emotion");
            Add("\U0001F621\uFE0F", "Enraged Red", "Smiley, Emotion");
            Add("\U0001F4A9\uFE0F", "Poo", "Smiley, Emotion");

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
            Add("\U0001F414\uFE0F", "Chicken", "Animal");
            Add("\U0001F9A4\uFE0F", "Dodo", "Animal");

            Add("\U0001F400\uFE0F", "Rat", "Animal");
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
            Add("\U0001F998\uFE0F", "Kangaroo", "Animal");

            Add("\U0001F40F\uFE0F", "Ram", "Animal");
            Add("\U0001F410\uFE0F", "Goat", "Animal");
            Add("\U0001F411\uFE0F", "Ewe", "Animal");
            Add("\U0001F417\uFE0F", "Boar", "Animal");
            Add("\U0001F98C\uFE0F", "Deer", "Animal");
            Add("\U0001FACE\uFE0F", "Moose", "Animal");
            Add("\U0001F9AC\uFE0F", "Bison", "Animal");
            Add("\U0001FACF\uFE0F", "Donkey", "Animal");
            Add("\U0001F999\uFE0F", "Llama", "Animal");
            Add("\U0001F42A\uFE0F", "Camel", "Animal");
            Add("\U0001F406\uFE0F", "Leopard", "Animal");
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
            Add("\U0001F984\uFE0F", "Unicorn", "Animal, Fantasy");

            // Marine Animals
            Add("\U0001F41F\uFE0F", "Fish", "Animal, Marine");
            Add("\U0001F420\uFE0F", "Tropical Fish", "Animal, Marine");
            Add("\U0001F421\uFE0F", "Blowfish", "Animal, Marine");
            Add("\U0001F988\uFE0F", "Shark", "Animal, Marine");
            Add("\U0001F40B\uFE0F", "Whale", "Animal, Marine");
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
            Add("\U0001F30B\uFE0F", "Volcano", "World, Landscape");
            Add("\U0001F3DC\uFE0F", "Desert", "World, Landscape");
            Add("\U0001F3DD\uFE0F", "Island", "World, Landscape");
            Add("\U0001F3D5\uFE0F", "Camping", "World, Landscape");
            Add("\U0001F3DE\uFE0F", "National Park", "World, Landscape");

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

            // Leisure
            Add("\U0001F3AA\uFE0F", "Circus Tent", "Structure, Leisure");
            Add("\U0001F3A0\uFE0F", "Carousel Horse", "Structure, Leisure");
            Add("\U0001F3A1\uFE0F", "Ferris Wheel", "Structure, Leisure");
            Add("\U0001F3A2\uFE0F", "Roller Coaster", "Structure, Leisure");
            Add("\U0001F6DD\uFE0F", "Playground Slide", "Structure, Leisure");

            // Furniture
            Add("\U0001F6AA\uFE0F", "Door", "Furniture");
            Add("\U0001FA9F\uFE0F", "Window", "Furniture");
            Add("\U0001F6CF\uFE0F", "Bed", "Furniture");
            Add("\U0001FA91\uFE0F", "Chair", "Furniture");
            Add("\U0001F5BC\uFE0F", "Picture", "Furniture");
            Add("\U0001F4EB\uFE0F", "Mailbox", "Furniture");
            Add("\U0001FAA7\uFE0F", "Signboard", "Furniture");
            Add("\U0001FAB4\uFE0F", "Potted Plant", "Furniture");

            // ===== Nature =====

            // Sky & Space
            Add("\U00002600\uFE0F", "Sun", "Nature, Sky");
            Add("\U0001F311\uFE0F", "Moon", "Nature, Sky");
            Add("\U0001F319\uFE0F", "Crescent Moon", "Nature, Sky");
            Add("\U00002B50\uFE0F", "Star", "Nature, Sky");
            Add("\U00002601\uFE0F", "Cloud", "Nature, Sky");
            Add("\U00002604\uFE0F", "Comet", "Nature, Space");
            Add("\U0001FA90\uFE0F", "Planet", "Nature, Space");

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

            // Plants
            Add("\U0001F331\uFE0F", "Seedling", "Nature, Plant");
            Add("\U0001F33E\uFE0F", "Wheat", "Nature, Plant");
            Add("\U0001F339\uFE0F", "Rose", "Nature, Plant");
            Add("\U0001F940\uFE0F", "Wilted Flower", "Nature, Plant");
            Add("\U0001F33B\uFE0F", "Sunflower", "Nature, Plant");
            Add("\U0001F33C\uFE0F", "Blossom", "Nature, Plant");
            Add("\U0001F338\uFE0F", "Cherry Blossom", "Nature, Plant");
            Add("\U0001F337\uFE0F", "Tulip", "Nature, Plant");
            Add("\U0001FABB\uFE0F", "Hyacinth", "Nature, Plant");
            Add("\U0001FAB7\uFE0F", "Lotus", "Nature, Plant");
            Add("\U0001F335\uFE0F", "Cactus", "Nature, Plant");

            // Trees
            Add("\U0001F333\uFE0F", "Deciduous Tree", "Nature, Tree");
            Add("\U0001F332\uFE0F", "Evergreen Tree", "Nature, Tree");
            Add("\U0001F334\uFE0F", "Palm Tree", "Nature, Tree");
            Add("\U0001FABE\uFE0F", "Leafless Tree", "Nature, Tree");

            // Leaves
            Add("\U0001F340\uFE0F", "Clover", "Nature, Leaf");
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
            Add("\U0001F52D\uFE0F", "Telescope", "Item, Knowledge");
            Add("\U0001F52C\uFE0F", "Microscope", "Item, Knowledge");

            Add("\U0001F528\uFE0F", "Hammer", "Item, Tool");
            Add("\U0001F527\uFE0F", "Wrench", "Item, Tool");
            Add("\U0001F6E0\uFE0F", "Hammer & Wrench", "Item, Tool");
            Add("\U000026CF\uFE0F", "Pick", "Item, Tool");
            Add("\U00002699\uFE0F", "Gear", "Item, Tool");
            Add("\U0001F9ED\uFE0F", "Compass", "Item, Tool");
            Add("\U0001F50D\uFE0F", "Magnifying Glass", "Item, Tool");
            Add("\U0001F9F2\uFE0F", "Magnet", "Item, Tool");
            Add("\U0001F50B\uFE0F", "Battery", "Item, Tool");
            Add("\U0001F9F9\uFE0F", "Broom", "Item, Tool");
            Add("\U0001F9FD\uFE0F", "Sponge", "Item, Tool");

            Add("\U0000231B\uFE0F", "Hourglass", "Item, Time");
            Add("\U000023F0\uFE0F", "Alarm Clock", "Item, Time");
            Add("\U00002696\uFE0F", "Justice Scales", "Item, Weight");

            Add("\U0001F3C6\uFE0F", "Trophy", "Item, Reward");
            Add("\U0001F3C5\uFE0F", "Medal", "Item, Reward");
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

            Add("\U00002709\uFE0F", "Envelope", "Item, Office");
            Add("\U0001F4E6\uFE0F", "Package", "Item, Office");
            Add("\U0001F4C5\uFE0F", "Calendar", "Item, Office");
            Add("\U0001F4C1\uFE0F", "File Folder", "Item, Office");
            Add("\U0001F5D2\uFE0F", "Spiral Notepad", "Item, Office");
            Add("\U0001F4CC\uFE0F", "Pushpin", "Item, Office");
            Add("\U0001F4CD\uFE0F", "Round Map Pin", "Item, Office");
            Add("\U0001FAAA\uFE0F", "ID Card", "Item, Office");
            Add("\U0001F5D1\uFE0F", "Wastebasket", "Item, Office");

            Add("\U0001F511\uFE0F", "Key", "Item, Access");
            Add("\U0001F5DD\uFE0F", "Old Key", "Item, Access");
            Add("\U0001F512\uFE0F", "Locked Padlock", "Item, Access");
            Add("\U0001F513\uFE0F", "Open Padlock", "Item, Access");
            Add("\U0001F6A8\uFE0F", "Alarm", "Item, Access");

            Add("\U000026B0\uFE0F", "Coffin", "Item, Death");
            Add("\U0001FAA6\uFE0F", "Headstone", "Item, Death");

            Add("\U0001F381\uFE0F", "Wrapped Gift", "Item, Leisure");
            Add("\U0001F389\uFE0F", "Party Popper", "Item, Leisure");
            Add("\U0001F388\uFE0F", "Balloon", "Item, Leisure");
            Add("\U0001F514\uFE0F", "Bell", "Item, Leisure");

            // Play
            Add("\U0001F3B2\uFE0F", "Die", "Play, Game");
            Add("\U0000265F\uFE0F", "Chess Pawn", "Play, Game");
            Add("\U0001F0CF\uFE0F", "Joker", "Play, Game");
            Add("\U0001F9E9\uFE0F", "Puzzle", "Play, Game");
            Add("\U0001F3AE\uFE0F", "Video Game", "Play, Game");
            Add("\U0001F3AF\uFE0F", "Bullseye", "Play, Game");
            Add("\U0001F945\uFE0F", "Goal", "Play, Game");

            Add("\U0001F3B5\uFE0F", "Music Note", "Play, Music");
            Add("\U0001F3B8\uFE0F", "Guitar", "Play, Music");
            Add("\U0001F941\uFE0F", "Drum", "Play, Music");

            Add("\U0001F3AD\uFE0F", "Performing Arts", "Play, Art");
            Add("\U0001F3A8\uFE0F", "Artist Palette", "Play, Art");

            // Tech
            Add("\U0001F4BB\uFE0F", "Computer", "Tech");
            Add("\U0001F5A5\uFE0F", "Desktop", "Tech");
            Add("\U0001F5B1\uFE0F", "Computer Mouse", "Tech");
            Add("\U0001F4BE\uFE0F", "Floppy Disk", "Tech");
            Add("\U0001F579\uFE0F", "Joystick", "Tech");
            Add("\U0001F4F1\uFE0F", "Mobile Phone", "Tech");
            Add("\U0000260E\uFE0F", "Telephone", "Tech");
            Add("\U0001F4FA\uFE0F", "TV", "Tech");
            Add("\U0001F4FB\uFE0F", "Radio", "Tech");
            Add("\U0001F4E1\uFE0F", "Antenna", "Tech");
            Add("\U0001F50C\uFE0F", "Plug", "Tech");
            Add("\U0001F4F7\uFE0F", "Camera", "Tech");
            Add("\U0001F3AC\uFE0F", "Clapperboard", "Tech");
            Add("\U0001F3A4\uFE0F", "Microphone", "Tech");
            Add("\U0001F3A7\uFE0F", "Headphone", "Tech");
            Add("\U0001F4E2\uFE0F", "Loudspeaker", "Tech");

            // ===== Food & Drink =====

            // Fruits
            Add("\U0001F34E\uFE0F", "Apple", "Food, Fruit");
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
            Add("\U0001F35C\uFE0F", "Noodle Soup", "Food");
            Add("\U0001F35D\uFE0F", "Spaghetti", "Food");
            Add("\U0001F35A\uFE0F", "Cooked Rice", "Food");
            Add("\U0001F363\uFE0F", "Sushi", "Food");
            Add("\U0001F95F\uFE0F", "Dumpling", "Food");
            Add("\U0001F9C6\uFE0F", "Falafel", "Food");
            Add("\U0001F355\uFE0F", "Pizza", "Food");
            Add("\U0001F354\uFE0F", "Burger", "Food");
            Add("\U0001F32D\uFE0F", "Hot Dog", "Food");
            Add("\U0001F96A\uFE0F", "Sandwich", "Food");
            Add("\U0001F32E\uFE0F", "Taco", "Food");
            Add("\U0001F32F\uFE0F", "Burrito", "Food");
            Add("\U0001F35F\uFE0F", "Fries", "Food");

            Add("\U0001F382\uFE0F", "Cake", "Food, Sweet");
            Add("\U0001F9C1\uFE0F", "Cupcake", "Food, Sweet");
            Add("\U0001F967\uFE0F", "Pie", "Food, Sweet");
            Add("\U0001F96E\uFE0F", "Moon Cake", "Food, Sweet");
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
            Add("\U0001F964\uFE0F", "Cup With Straw", "Drink");
            Add("\U0001F9C3\uFE0F", "Juice Box", "Drink");
            Add("\U0001F9CB\uFE0F", "Bubble Tea", "Drink");
            Add("\U0001F9CA\uFE0F", "Ice Cube", "Drink");
            Add("\U0001F37A\uFE0F", "Beer", "Drink");
            Add("\U0001F377\uFE0F", "Wine", "Drink");
            Add("\U0001F37E\uFE0F", "Champagne", "Drink");
            Add("\U0001F376\uFE0F", "Sake", "Drink");
            Add("\U0001F943\uFE0F", "Tumbler", "Drink");
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
            Add("\U0001F681\uFE0F", "Helicopter", "Mobility, Air");
            Add("\U0001F680\uFE0F", "Rocket", "Mobility, Air");
            Add("\U0001F6F0\uFE0F", "Satellite", "Mobility, Air");
            Add("\U0001F6F8\uFE0F", "UFO", "Mobility, Air");

            // ===== Symbols =====

            // Signs
            Add("\U0001F6D1\uFE0F", "Stop Sign", "Symbol, Sign");
            Add("\U0001F6A7\uFE0F", "Construction Sign", "Symbol, Sign");
            Add("\U00002668\uFE0F", "Hot Springs", "Symbol, Sign");

            // Status
            Add("\U00002705\uFE0F", "Check", "Symbol, Status");
            Add("\U0000274C\uFE0F", "Cross Mark", "Symbol, Status");
            Add("\U00002753\uFE0F", "Question", "Symbol, Status");
            Add("\U00002757\uFE0F", "Exclamation", "Symbol, Status");
            Add("\U000026A0\uFE0F", "Warning", "Symbol, Status");
            Add("\U000026D4\uFE0F", "No Entry", "Symbol, Status");
            Add("\U0001F6AB\uFE0F", "Prohibited", "Symbol, Status");
            Add("\U00002622\uFE0F", "Radioactive", "Symbol, Status");
            Add("\U00002623\uFE0F", "Biohazard", "Symbol, Status");
            Add("\U00002695\uFE0F", "Medical", "Symbol, Status");

            // Markers
            Add("\U0001F534\uFE0F", "Red Circle", "Symbol, Marker");
            Add("\U0001F7E0\uFE0F", "Orange Circle", "Symbol, Marker");
            Add("\U0001F7E1\uFE0F", "Yellow Circle", "Symbol, Marker");
            Add("\U0001F7E2\uFE0F", "Green Circle", "Symbol, Marker");
            Add("\U0001F535\uFE0F", "Blue Circle", "Symbol, Marker");
            Add("\U0001F7E3\uFE0F", "Purple Circle", "Symbol, Marker");
            Add("\U000026AB\uFE0F", "Black Circle", "Symbol, Marker");
            Add("\U000026AA\uFE0F", "White Circle", "Symbol, Marker");

            // Expressions
            Add("\U0001F4AC\uFE0F", "Speech Bubble", "Symbol");
            Add("\U0001F4AD\uFE0F", "Thought Bubble", "Symbol");
            Add("\U0001F5EF\uFE0F", "Anger Bubble", "Symbol");
            Add("\U0001F4A4\uFE0F", "ZZZ", "Symbol");
            Add("\U0001F4AF\uFE0F", "100", "Symbol");
            Add("\U00002764\uFE0F", "Heart", "Symbol");
            Add("\U0001F494\uFE0F", "Broken Heart", "Symbol");
            Add("\U0001F48B\uFE0F", "Kiss", "Symbol");
            Add("\U00002728\uFE0F", "Sparkles", "Symbol");
            Add("\U0001F4AB\uFE0F", "Dizzy", "Symbol");
            Add("\U0001F4A5\uFE0F", "Collision", "Symbol");
            Add("\U0001F4A6\uFE0F", "Droplets", "Symbol");
            Add("\U0000267E\uFE0F", "Infinity", "Symbol");
            Add("\U0001F517\uFE0F", "Link", "Symbol");
            Add("\U0000267B\uFE0F", "Recycling Symbol", "Symbol");

            // Faith
            Add("\U0000271D\uFE0F", "Latin Cross", "Symbol, Faith");
            Add("\U0000262A\uFE0F", "Star & Crescent", "Symbol, Faith");
            Add("\U00002721\uFE0F", "Star of David", "Symbol, Faith");
            Add("\U0001F549\uFE0F", "Om", "Symbol, Faith");
            Add("\U00002638\uFE0F", "Dharma Wheel", "Symbol, Faith");
            Add("\U0000262F\uFE0F", "Yin Yang", "Symbol, Faith");
            Add("\U0000262E\uFE0F", "Peace", "Symbol, Faith");

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
