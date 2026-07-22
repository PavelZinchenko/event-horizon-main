using System.Linq;
using UnityEngine;
using Zenject;
using GameDatabase;
using GameDatabase.Model;
using GameDatabase.DataModel;
using ShipEditor.Context;
using Constructor.Ships;
using ShipEditor.Model;

namespace Installers
{
	public class ShipEditorSceneInstaller : MonoInstaller<ShipEditorSceneInstaller>
	{
		[Inject] IDatabase _database;

		[SerializeField] private int _testShipBuildId = 316;
		[SerializeField] private bool _lockTestShipModules = true;

		public override void InstallBindings()
		{
			Container.Bind<IShipEditorContext>().FromMethod(CreateTestContext).AsSingle().IfNotBound();
			Container.BindInterfacesTo<ShipEditorModel>().AsSingle();
			Container.Bind<CommandList>().AsSingle();
		}

		private IShipEditorContext CreateTestContext(InjectContext injectContext)
		{
			var shipBuild = _database.GetShipBuild(ItemId<ShipBuild>.Create(_testShipBuildId));
			if (shipBuild == null) shipBuild = _database.ShipBuildList.First();

			return new DatabaseEditorContext(_database, _lockTestShipModules ? new CommonShip(shipBuild, _database) : new EditorModeShip(shipBuild, _database));
		}
	}
}
