using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FluentAutomation.Tests.Remote
{
    public class IOSTests : FluentTest
    {
        [Fact]
        public void iPhone()
        {
            FluentAutomation.SeleniumWebDriver.Bootstrap(new Uri("http://10.0.1.25:8080/"), SeleniumWebDriver.Browser.InternetExplorer);

            I.Open("http://automation.apphb.com/forms");
            I.Select("Motorcycles").From(".liveExample tr select:eq(0)"); // Select by value/text
            I.Select(2).From(".liveExample tr select:eq(1)"); // Select by index
            I.Enter(6).In(".liveExample td.quantity input:eq(0)");
            I.Expect.Text("$197.70").In(".liveExample tr span:eq(1)");

            // add second product
            I.Click(".liveExample button:eq(0)");
            I.Select(1).From(".liveExample tr select:eq(2)");
            I.Select(4).From(".liveExample tr select:eq(3)");
            I.Enter(8).In(".liveExample td.quantity input:eq(1)");
            I.Expect.Text("$788.64").In(".liveExample tr span:eq(3)");

            // validate totals
            I.Expect.Text("$986.34").In("p.grandTotal span");

            // remove first product
            I.Click(".liveExample a:eq(0)");

            // validate new total
            I.WaitUntil(() => I.Expect.Text("$788.64").In("p.grandTotal span"));
        }
    }
}
