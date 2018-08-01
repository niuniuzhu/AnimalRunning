using System.Collections.Generic;

namespace Logic
{
	public static class ModelFactory
	{
		private static readonly Dictionary<string, MapData> MAP_DATA = new Dictionary<string, MapData>();
		private static readonly Dictionary<string, EntityData> ENTITY_DATA = new Dictionary<string, EntityData>();
		private static readonly Dictionary<string, BuffData> BUFF_DATA = new Dictionary<string, BuffData>();
		private static readonly Dictionary<string, EffectData> EFFECT_DATA = new Dictionary<string, EffectData>();

		public static MapData GetMapData( string id )
		{
			if ( MAP_DATA.TryGetValue( id, out MapData data ) )
				return data;

			data = new MapData( id );
			MAP_DATA[id] = data;
			return data;
		}

		public static EntityData GetEntityData( string id )
		{
			if ( ENTITY_DATA.TryGetValue( id, out EntityData data ) )
				return data;

			data = new EntityData( id );
			ENTITY_DATA[id] = data;
			return data;
		}

		public static BuffData GetBuffData( string id )
		{
			if ( BUFF_DATA.TryGetValue( id, out BuffData data ) )
				return data;

			data = new BuffData( id );
			BUFF_DATA[id] = data;
			return data;
		}

		public static EffectData GetEffectData( string id )
		{
			if ( EFFECT_DATA.TryGetValue( id, out EffectData data ) )
				return data;

			data = new EffectData( id );
			EFFECT_DATA[id] = data;
			return data;
		}
	}
}