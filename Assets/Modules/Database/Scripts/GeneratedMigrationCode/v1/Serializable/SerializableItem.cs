//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
    [Serializable]
    public class SerializableItem
    {
	    public string FileName { get; set; }
        public ItemType ItemType;
        public int Id;
        public bool Disabled;
    }
}
