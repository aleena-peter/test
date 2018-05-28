using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NLog;

namespace Configit.AGCO.Prototype.KMATImporter {
  public class MaterialImporter {
    private readonly PackageInfo _packageInfo;
    public string ModelBinDir { get; }

    private const string ImportKMATexe = "Configit.ImportKMAT.exe";
    private const string dbnew = "dbnew.exe";
    private const string db2xml = "db2xml.exe";
    private const string modelFolder = @"App\Materials\Models";

    private readonly Logger Log = LogManager.GetLogger( "Material Importer" );
    private readonly Logger ProcessLog = LogManager.GetLogger( ">" );

    public MaterialImporter( PackageInfo packageInfo, string modelBinDir ) {
      _packageInfo = packageInfo;
      ModelBinDir = modelBinDir;
    }

    public string CreateModelFile( string productModelName, bool overwrite = false ) {
      var modelFileName = $"{productModelName}.model";
      if ( !overwrite && File.Exists( modelFileName ) ) {
        Log.Info( $"model {modelFileName} already exisits, skipping" );
        return modelFileName;
      }
      File.Delete( modelFileName );
      RunProcess( dbnew, $" {modelFileName}" );
      return Path.GetFullPath( modelFileName );
    }

    public void ImportKMAT( string modelFile, string productModelName ) {
      RunProcess( ImportKMATexe, $" -models {Path.Combine( _packageInfo.PackagePath, modelFolder )} -pm {productModelName} -file \"{modelFile}\"" );
    }

    public string CreatePMX( string modelFile, bool overwrite = false ) {
      var modelName = Path.GetFileNameWithoutExtension( modelFile );
      var pmxFileName = modelName + ".pmx";
      if ( !overwrite && File.Exists( pmxFileName ) ) {
        Log.Info( $"PMX {pmxFileName} already exisits, skipping" );
      }
      else {
        RunProcess( db2xml, $" -o \"{pmxFileName}\" -pm {modelName} \"{modelFile}\"" );
      }
      return pmxFileName;
    }

    private bool RunProcess( string processName, string arguments ) {
      Log.Info( $"Starting process {processName} {arguments}" );
      var psi = new ProcessStartInfo( Path.Combine( ModelBinDir, processName ), arguments ) {
        UseShellExecute = false,
        RedirectStandardOutput = true,
      };
      var process = new Process() {
        StartInfo = psi,
      };
      process.OutputDataReceived += ( sender, args ) => ProcessLog.Debug( args.Data );
      process.Start();
      process.BeginOutputReadLine();

      if ( process == null ) {
        throw new ImportException( $"Could not start process '${processName}' with arguments '{arguments}' " );
      }
      process.WaitForExit();
      if ( process.ExitCode != 0 ) {
        throw new ImportException( $"Process '${processName}' with arguments '{arguments}' did not terminate successfully" );
      }
      return true;
    }



  }
}
