namespace NBsoft.Wordzz.Extensions
{
    internal static class ContractExtensions
    {  
        public static T ToDto<T>(this object src) where T : new()
        {
            var res = new T();
            foreach (var prop in src.GetType().GetProperties())
            {
                var value = prop.GetValue(src);
                prop.SetValue(res, value, null);
                
            }

            return res;
        }
    }
}
