using Xunit;

namespace GlobbingTests.Helpers
{
    public class TheoryWindowsOnlyAttribute : TheoryAttribute
    {
#if NOT_WINDOWS
        public FactWindowsOnlyAttribute() 
        {
            Skip = "Ignored on other than Windows";
        }
#endif
    }
}
