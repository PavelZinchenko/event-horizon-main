namespace GameDatabase.Storage
{
    public partial interface IDataStorage
    {
        string Name { get; }
        string Id { get; }
        Version Version { get; }
        bool IsEditable { get; }

        void UpdateItem(string name, string content);
    }

    public readonly struct Version
    {
        public Version(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public readonly int Major;
        public readonly int Minor;
    }
}
