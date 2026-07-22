using System;
using Zenject;
using GameDatabase;

namespace Gui.Theme
{
    public class UiThemeLoader : IInitializable, IDisposable
    {
        private readonly IDatabase _database;
        private readonly UiTheme _defaultTheme;

        public UiThemeLoader(IDatabase database, UiTheme defaultTheme)
        {
            _database = database;
            _defaultTheme = defaultTheme;
            UiTheme.Current = defaultTheme;
        }

        public void Initialize()
        {
            _database.DatabaseLoaded += OnDatabaseLoaded;
            OnDatabaseLoaded();
        }

        public void Dispose()
        {
            _database.DatabaseLoaded -= OnDatabaseLoaded;
        }

        private void OnDatabaseLoaded()
        {
            if (_database.UiSettings == null) return;
            _defaultTheme.Import(_database);
        }
    }
}
