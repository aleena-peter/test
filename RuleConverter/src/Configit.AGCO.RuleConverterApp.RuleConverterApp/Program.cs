using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Configit.AGCO.Prototype.KMATImporter;
using Configit.AGCO.Prototype.RuleConverter;
using Configit.AGCO.Prototype.RuleConverter.Data;
using Configit.Model.Serialization;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Configit.AGCO.Prototype.RuleConverterApp {
  class Program {
    private const string ModelBinDir = @"C:\Program Files (x86)\Configit Model 5.11\Bin";

    /* Examples
     * -i C:\Users\robin\Documents\customers\AGCO\Data\Package\MF7726-quotepackage
     * -d C:\Users\robin\Documents\customers\AGCO\Data\Models\demo\MF7726D6_AGCO.vtpackage\
     */

    private static readonly Logger Log = LogManager.GetLogger( "RuleParseApp" );

    static void Main( string[] args ) {
      ConfigureLogger();
      var materialConverter = new MaterialConverter( Log, ModelBinDir );

      var options = new Options();
      if ( CommandLine.Parser.Default.ParseArguments( args, options ) ) {
        if ( options.QuotePackage != null ) {
          var packageInfo = new PackageInfo( options.QuotePackage );
          materialConverter.ImportMaterialsAndRules( packageInfo );
        }
        if ( options.VtPackage != null ) {
          var materialNbr = "MF7726D6"; //TODO add as parameter
          Log.Info( $"Importing defaults for {materialNbr}" );
          materialConverter.AddDefaults( options.VtPackage, materialNbr );
        }
      }

      //Log.Info( "Done!" );
      //Console.ReadKey();
    }


    private static void ConfigureLogger() {
      var config = new LoggingConfiguration();
      // Console logger
      var consoleTarget = new ColoredConsoleTarget();
      config.AddTarget( "console", consoleTarget );
      // File logger
      var fileTarget = new FileTarget();
      config.AddTarget( "file", fileTarget );
      consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
      fileTarget.FileName = "${basedir}/file.txt";
      fileTarget.Layout = "${message}";
      // Define rules
      var rule1 = new LoggingRule( "*", LogLevel.Debug, consoleTarget );
      config.LoggingRules.Add( rule1 );

      var rule2 = new LoggingRule( "*", LogLevel.Debug, fileTarget );
      config.LoggingRules.Add( rule2 );

      // Step 5. Activate the configuration
      LogManager.Configuration = config;
    }

  }
}
