
    public static class FilePathConsts
    {
        // ConfigFilePath will get overwritten if a different path is passed into Program.cs from the command line
        // - eg if you are running the code in .Net Core 
        // (particularly important on a Mac, where C:/Config will not work)
        public static string ConfigFilePath = @"C:/Config";
        public static string ConfigFileName = "Config.xml";
        public static string SampleConfigFileName = "SampleConfig.xml";
        public static string ConfigPathProperty = "MainConfigFilePath";
    }