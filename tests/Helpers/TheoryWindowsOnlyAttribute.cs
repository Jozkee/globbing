using Xunit;

namespace GlobbingTests.Helpers
{
    public class TheoryWindowsOnlyAttribute : TheoryAttribute
    {
#if NOT_WINDOWS
        public TheoryWindowsOnlyAttribute() 
        {
            Skip = "Ignored on other than Windows";
        }
#endif
    }
}
