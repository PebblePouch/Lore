namespace PebblePouch.Lore
{
    public enum FieldType
    {
        Text = 0,

        Entry = 1,
        GameObject = 2,
        Sprite = 3,

        Boolean = 4,
        Integer = 5,
        Float = 6,

        Scene = 7,

        Url = 8,

        // Reference to a ScriptableObject of a type chosen per Field (see Field.ObjectType).
        ScriptableObject = 9,

        // One of the options defined on the Field (see Field.EnumOptions); values store the option ID.
        Enum = 10,

        // Several Entry references in one Field (ordered, no duplicates), e.g. "Allies", "Appears in".
        Entries = 11,

        // An AudioClip (.wav, .mp3, .ogg, ...).
        Audio = 12,
    }
}