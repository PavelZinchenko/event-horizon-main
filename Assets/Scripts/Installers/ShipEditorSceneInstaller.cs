using System.Collections.Generic;
using System.Linq;
using Zenject;
using CommonComponents.Signals;
using Services.Messenger;
using GameServices.SceneManager;
using GameDatabase;
using GameDatabase.Model;
using GameDatabase.DataModel;
using ShipEditor.Context;
using CommonComponents.Utils;
using Constructor.Ships;
using Constructor;
using ShipEditor.Model;
using Services.ObjectPool;
using Economy;

namespace Installers
{
	public class ShipEditorSceneInstaller : MonoInstaller<ShipEditorSceneInstaller>
	{
		[Inject] IDatabase _database;

		public override void InstallBindings()
		{
			Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsCached();
			Container.Bind<IShipEditorContext>().FromMethod(CreateTestContext).AsSingle().IfNotBound();
			Container.BindInterfacesTo<ShipEditorModel>().AsSingle();
			Container.Bind<CommandList>().AsSingle();
		}

		private IShipEditorContext CreateTestContext(InjectContext injectContext)
		{
			return new DatabaseEditorContext(_database, 
				new CommonShip(_database.GetShipBuild(ItemId<ShipBuild>.Create(316))));
		}
	}
}
