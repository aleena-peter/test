using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Configit.AGCO.Prototype.RuleConverterApp {
  class Options {
    [Option( 'i', "all", HelpText = "Path to the quote package" )]
    public string QuotePackage { get; set; }

    [Option( 'd', "all", HelpText = "Path to the VT package" )]
    public string VtPackage { get; set; }
  }



}
