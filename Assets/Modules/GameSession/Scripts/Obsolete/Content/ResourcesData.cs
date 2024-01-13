using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using GameModel.Serialization;
using CommonComponents.Utils;

namespace Session.ContentObsolete
{
    public class ResourcesData : ISerializableDataObsolete
	{
        [Inject]
        public ResourcesData(byte[] buffer = null)
        {
            IsChanged = true;
			_money = 0;
			_stars = 0;
			_tokens = 0;
			_fuel = 0;

			if (buffer != null && buffer.Length > 0)
                Deserialize(buffer);

            _resources.CollectionChangedEvent += OnCollectionChanged;
        }

        public string FileName => Name;
        public const string Name = "resources";

        public bool IsChanged { get; private set; }
		public static int CurrentVersion => 6;

		public long Money 
		{
			get { return _money; }
			set
			{
                if (_money == value)
                    return;

			    IsChanged = true;
				_money = value; 
			}
		}
		
		public int Fuel
		{
			get { return _fuel; }
			set
			{
                if (_fuel == value)
                    return;

				IsChanged = true;
				_fuel = value;
			}
		}

        public int Tokens
        {
            get { return _tokens; }
            set
            {
                if (_tokens == value)
                    return;

                IsChanged = true;
                _tokens = value;
            }
        }

        public long Stars
		{
			get { return _stars; }
			set
			{
                if (_stars == value)
                    return;

			    IsChanged = true;
				_stars = value; 
			}
		}

	    public IGameItemCollection<int> Resources { get { return _resources; } }

		public IEnumerable<byte> Serialize()
		{
			IsChanged = false;
			
			foreach (var value in BitConverter.GetBytes(CurrentVersion))
				yield return value;

			foreach (var value in Helpers.Serialize(Money))
				yield return value;
			foreach (var value in Helpers.Serialize(Stars))
				yield return value;
			foreach (var value in Helpers.Serialize(Fuel))
				yield return value;
            foreach (var value in Helpers.Serialize(Tokens))
                yield return value;
		    foreach (var value in Helpers.Serialize(_resources))
		        yield return value;

		    foreach (var value in Helpers.Serialize(0)) // reserved
		        yield return value;
		    foreach (var value in Helpers.Serialize(0))
		        yield return value;
		    foreach (var value in Helpers.Serialize(0))
		        yield return value;
		    foreach (var value in Helpers.Serialize(0))
		        yield return value;
        }

        private void Deserialize(byte[] buffer)
		{
			if (buffer == null || buffer.Length == 0)
                throw new ArgumentException();

			int index = 0;
			var version = Helpers.DeserializeInt(buffer, ref index);
			if (version != CurrentVersion && !TryUpgrade(ref buffer, version))
			{
				UnityEngine.Debug.Log("ResourcesData: incorrect data version");
                throw new ArgumentException();
            }

			_money = Helpers.DeserializeLong(buffer, ref index);
			_stars = Helpers.DeserializeLong(buffer, ref index);
			_fuel = Helpers.DeserializeInt(buffer, ref index);
            _tokens = Helpers.DeserializeInt(buffer, ref index);
		    _resources.Assign(Helpers.DeserializeDictionary(buffer, ref index));

#if UNITY_EDITOR
			UnityEngine.Debug.Log("ResourcesData: money = " + _money);
			UnityEngine.Debug.Log("ResourcesData: fuel = " + _fuel);
			UnityEngine.Debug.Log("ResourcesData: stars = " + _stars);
            UnityEngine.Debug.Log("ResourcesData: tokens = " + _tokens);
#endif

            IsChanged = false;
		}

		private static bool TryUpgrade(ref byte[] data, int version)
		{
			if (version == 1)
			{
				data = Upgrade_1_2(data).ToArray();
				version = 2;
			}

			if (version == 2)
			{
				data = Upgrade_2_3(data).ToArray();
				version = 3;
			}

            if (version == 3)
            {
                data = Upgrade_3_4(data).ToArray();
                version = 4;
            }

		    if (version == 4)
		    {
		        data = Upgrade_4_5(data).ToArray();
		        version = 5;
		    }

			if (version == 5)
			{
				data = Upgrade_5_6(data).ToArray();
				version = 6;
			}

			return version == CurrentVersion;
		}
		
		private static IEnumerable<byte> Upgrade_1_2(byte[] buffer)
		{
			int index = 0;
			
			Helpers.DeserializeInt(buffer, ref index);
			var version = 2;
			foreach (var value in Helpers.Serialize(version))
				yield return value;				

			for (int i = index; i < buffer.Length; ++i)
				yield return buffer[i];

			foreach (var value in Helpers.Serialize(0)) // commonResourcesCount
				yield return value;
		}

		private static IEnumerable<byte> Upgrade_2_3(byte[] buffer)
		{
			int index = 0;

			Helpers.DeserializeInt(buffer, ref index);
			var version = 3;
			foreach (var value in Helpers.Serialize(version))
				yield return value;				

			var money = Helpers.DeserializeInt(buffer, ref index);
			var fuel = Helpers.DeserializeInt(buffer, ref index);
			var stars = Helpers.DeserializeInt(buffer, ref index);

			var count = Helpers.DeserializeInt(buffer, ref index);
			for (int i = 0; i < count; ++i)
			{
				var key = Helpers.DeserializeInt(buffer, ref index);
				var value = Helpers.DeserializeInt(buffer, ref index);
				stars += (value+4)/5;
			}

			foreach (var value in Helpers.Serialize(money))
				yield return value;				
			foreach (var value in Helpers.Serialize(fuel))
				yield return value;
			foreach (var value in Helpers.Serialize(stars))
				yield return value;				

			for (int i = index; i < buffer.Length; ++i)
				yield return buffer[i];
		}

        private static IEnumerable<byte> Upgrade_3_4(byte[] buffer)
        {
            int index = 0;

            Helpers.DeserializeInt(buffer, ref index); // version
            foreach (var value in Helpers.Serialize(4))
                yield return value;

            var money = Helpers.DeserializeInt(buffer, ref index);
            var fuel = Helpers.DeserializeInt(buffer, ref index);
            var stars = Helpers.DeserializeInt(buffer, ref index);

            foreach (var value in Helpers.Serialize(money))
                yield return value;
            foreach (var value in Helpers.Serialize(fuel))
                yield return value;
            foreach (var value in Helpers.Serialize(stars))
                yield return value;
            foreach (var value in Helpers.Serialize(0)) // tokens
                yield return value;
            foreach (var value in Helpers.Serialize(0)) // reserved
                yield return value;

            for (int i = index; i < buffer.Length; ++i)
                yield return buffer[i];
        }

	    private static IEnumerable<byte> Upgrade_4_5(byte[] buffer)
	    {
	        int index = 0;

	        Helpers.DeserializeInt(buffer, ref index); // version
	        foreach (var value in Helpers.Serialize(5))
	            yield return value;

	        var money = Helpers.DeserializeInt(buffer, ref index);
	        var fuel = Helpers.DeserializeInt(buffer, ref index);
	        var stars = Helpers.DeserializeInt(buffer, ref index);
	        var tokens = Helpers.DeserializeInt(buffer, ref index);

	        foreach (var value in Helpers.Serialize(money))
	            yield return value;
	        foreach (var value in Helpers.Serialize(fuel))
	            yield return value;
	        foreach (var value in Helpers.Serialize(stars))
	            yield return value;
	        foreach (var value in Helpers.Serialize(tokens))
	            yield return value;

	        var resources = Helpers.DeserializeDictionary(buffer, ref index);
	        var commonResources = Helpers.DeserializeDictionary(buffer, ref index);

	        foreach (var item in commonResources)
	        {
	            int value;
                if (resources.TryGetValue(item.Key, out value))
                    resources[item.Key] = value + item.Value;
                else
                    resources.Add(item.Key, item.Value);
	        }

	        foreach (var value in Helpers.Serialize(resources))
	            yield return value;

	        foreach (var value in Helpers.Serialize(0)) // reserved
	            yield return value;
	        foreach (var value in Helpers.Serialize(0)) // reserved
	            yield return value;
	        foreach (var value in Helpers.Serialize(0)) // reserved
	            yield return value;
	        foreach (var value in Helpers.Serialize(0)) // reserved
	            yield return value;
        }

		private static IEnumerable<byte> Upgrade_5_6(byte[] buffer)
		{
			int index = 0;

			Helpers.DeserializeInt(buffer, ref index); // version
			foreach (var value in Helpers.Serialize(6))
				yield return value;

			var money = Helpers.DeserializeInt(buffer, ref index);
			var fuel = Helpers.DeserializeInt(buffer, ref index);
			var stars = Helpers.DeserializeInt(buffer, ref index);
			var tokens = Helpers.DeserializeInt(buffer, ref index);

			foreach (var value in Helpers.Serialize((long)money))
				yield return value;
			foreach (var value in Helpers.Serialize((long)stars))
				yield return value;
			foreach (var value in Helpers.Serialize(fuel))
				yield return value;
			foreach (var value in Helpers.Serialize(tokens))
				yield return value;

			for (int i = index; i < buffer.Length; ++i)
				yield return buffer[i];
		}

		private void OnCollectionChanged()
	    {
	        IsChanged = true;
	    }

        private ObscuredLong _money;
		private ObscuredLong _stars;
		private ObscuredInt _fuel;
        private ObscuredInt _tokens;
	    internal readonly GameItemCollection<int> _resources = new GameItemCollection<int>();
	}
}
