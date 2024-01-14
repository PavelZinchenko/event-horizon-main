//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session
{
	public partial class SessionLoader
	{

		public Model.SaveGameData Convert(Model.SaveGameData data)
		{
			return data;
		}

		public Model.SaveGameData Load(SessionDataReader reader, int versionMajor, int versionMinor)
		{
			Model.SaveGameData data1 = null;
			if (versionMajor == 1)
				data1 = new Model.SaveGameData(reader, null);

			return data1;
		}
	
	}
}
