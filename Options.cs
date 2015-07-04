using CommandLine;
using CommandLine.Text;

namespace Trizbort
{
  public class Options
  {
    [Option('f',"open", Required =true, HelpText = "Trizbort file to load.")]
    public string TrizbortFile { get; set; } = string.Empty;

    [Option("script", HelpText = "Load specified script file and run.")]
    public string ScriptFile { get; set; } = string.Empty;

    [HelpOption]
    public string GetUsage()
    {
      string s = HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
      return s;
    }
  }
}