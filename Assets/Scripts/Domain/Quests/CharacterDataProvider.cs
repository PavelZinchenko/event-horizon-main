using GameDatabase.DataModel;
using GameDatabase.Model;
using Session;

namespace Domain.Quests
{
    public class CharacterDataProvider : ICharacterDataProvider
    {
        private readonly ISessionData _session;

        public CharacterDataProvider(ISessionData session)
        {
            _session = session;
        }

        public int GetCharacterAttitude(ItemId<Character> id) => _session.Quests.GetCharacterRelations(id.Value);
    }
}
