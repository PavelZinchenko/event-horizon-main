namespace DatabaseCodeGenerator.Schema
{
    public static class XmlClassMemberExtensions
    {
        public static void MinMaxInt(this XmlClassMember member, out int minvalue, out int maxvalue)
        {
            if (string.IsNullOrEmpty(member.minvalue) || !int.TryParse(member.minvalue, out minvalue)) minvalue = int.MinValue;
            if (string.IsNullOrEmpty(member.maxvalue) || !int.TryParse(member.maxvalue, out maxvalue)) maxvalue = int.MaxValue;
        }

        public static void MinMaxFloat(this XmlClassMember member, out float minvalue, out float maxvalue)
        {
            if (string.IsNullOrEmpty(member.minvalue) || !float.TryParse(member.minvalue, out minvalue)) minvalue = float.MinValue;
            if (string.IsNullOrEmpty(member.maxvalue) || !float.TryParse(member.maxvalue, out maxvalue)) maxvalue = float.MaxValue;
        }
    }
}
