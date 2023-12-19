using System;
using System.Collections.Generic;
using Combat.Component.Bullet;
using Combat.Factory;

namespace Combat.Component.Systems.Weapons
{
	public interface IBulletCompositeDisposable : IDisposable
	{
		void Add(IBullet bullet);
	}

	public class BulletCompositeDisposable : IBulletCompositeDisposable
	{
		public static IBulletCompositeDisposable Create(IBulletStats bulletStats)
		{
			return bulletStats.IsBoundToCannon ? new BulletCompositeDisposable() : _stub;
		}

		public void Add(IBullet bullet)
		{
			_bullets.RemoveAll(IsNotActive);
			_bullets.Add(bullet);
		}

		public void Dispose()
		{
			foreach (var bullet in _bullets)
				bullet.Vanish();
		}

		private static bool IsNotActive(IBullet bullet) => bullet.State != Combat.Unit.UnitState.Active;

		private readonly List<IBullet> _bullets = new();
		private static Stub _stub = new();

		private class Stub : IBulletCompositeDisposable
		{
			public void Add(IBullet bullet) { }
			public void Dispose() { }
		}
	}
}
