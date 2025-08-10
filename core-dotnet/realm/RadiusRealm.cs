namespace JRadius.Core.Realm
{
    public class RadiusRealm : JRadiusRealm
    {
        public RadiusRealm()
        {
            // The IsLocal property is calculated in the original Java code.
            // In C#, it's better to make it a get-only property.
            // I will update the JRadiusRealm class to reflect this.
        }

        public override string ToString()
        {
            return Realm;
        }
    }
}
