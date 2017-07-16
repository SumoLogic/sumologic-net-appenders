using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2098d9a3-20a5-4c48-bc3e-6c0821452d3d")]

[assembly: CLSCompliant(true)]

// This will cause log4net to look for a configuration file 
// called [ThisApp].exe.config in the application base 
// directory (i.e. the directory containing [ThisApp].exe) 
// The config file will be watched for changes. 
[assembly: log4net.Config.XmlConfigurator(Watch = true)]