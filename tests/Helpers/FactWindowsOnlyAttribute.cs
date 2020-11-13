using Xunit;

namespace GlobbingTests.Helpers
{
    public class FactWindowsOnlyAttribute : FactAttribute
    {
#if NOT_WINDOWS
        public FactWindowsOnlyAttribute() 
        {
            Skip = "Ignored on other than Windows";
        }
#endif
    }
}
