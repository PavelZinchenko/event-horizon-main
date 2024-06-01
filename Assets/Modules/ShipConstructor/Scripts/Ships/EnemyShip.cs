using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase;

namespace Constructor.Ships
{
    public class EnemyShip : CommonShip
    {
        public EnemyShip(ShipBuild data, IDatabase database) 
            : base(data, database)
        {
            ExtraThreatLevel = data.DifficultyClass;
        }

        public override DifficultyClass ExtraThreatLevel { get; }

        public override ShipBuilder CreateBuilder()
        {
            var builder = base.CreateBuilder();

            if (ExtraThreatLevel != DifficultyClass.Default)
                builder.Converter = new EnemyComponentConverter(Experience.Level, new System.Random((int)Experience));

            return builder;
        }
    }
}
