using System;
using Xunit;

namespace CheckMarkdownLinks.Tests
{
    public class RunTests
    {
        
        [Fact]
        public void GetLinkType_should_detect_correct_type()
        {
            var systemUnderTest = new Run();

            Assert.Equal(LinkType.Anchor, systemUnderTest.GetLinkType("#anchor"));
            Assert.Equal(LinkType.External, systemUnderTest.GetLinkType("https://www.microsoft.com"));
            Assert.Equal(LinkType.External, systemUnderTest.GetLinkType("https://www.microsoft.com/stuff/helloworld"));
            Assert.Equal(LinkType.External, systemUnderTest.GetLinkType("http://www.microsoft.com"));
            Assert.Equal(LinkType.Internal, systemUnderTest.GetLinkType("./sub/sub/file.txt"));
            Assert.Equal(LinkType.Internal, systemUnderTest.GetLinkType("file.txt"));
            Assert.Equal(LinkType.Internal, systemUnderTest.GetLinkType("../sub/sub/file.txt"));
            Assert.Equal(LinkType.Internal, systemUnderTest.GetLinkType("/root/sub/file.txt"));
            Assert.Equal(LinkType.Internal, systemUnderTest.GetLinkType("/root/sub/file.txt#anchor"));

        }         
    }
}
