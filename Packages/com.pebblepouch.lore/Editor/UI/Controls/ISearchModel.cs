namespace PebblePouch.Lore.Editor
{
    public interface ISearchModel
    {
        string Search { get; set; }

        void Refresh();
    }
}