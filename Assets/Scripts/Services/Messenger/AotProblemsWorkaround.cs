namespace Services.Messenger
{
	internal static class AotProblemsWorkaround
	{
		private static void UsedOnlyForAOTCodeGeneration()
		{
			var context = new MessengerContext();
			context.AddListener<Account.Status>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<int>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Constructor.Ships.IShipModel>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Constructor.Ships.IShip>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Combat.Component.Ship.IShip>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Combat.Component.Unit.IUnit>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<CommonComponents.Money>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<GameDatabase.Model.ItemId<GameDatabase.DataModel.Ship>>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Economy.Products.IProduct>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<bool>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Storage.CloudStorageStatus>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<string>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<GameServices.Player.ViewMode>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<GameServices.Multiplayer.Status>(EventType.MultiplayerStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<GameServices.Multiplayer.IPlayerInfo>(EventType.MultiplayerStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Galaxy.StarObjectType>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<Advertisements.AdStatus>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, value => { });
			context.AddListener<int,int,float>(EventType.AccountStatusChanged, GameServices.SceneManager.GameScene.Loader, (value1,value2,value3) => { });
		}
	}
}
