using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TMTech.MapService.Infrastructure;

namespace Ming.Tools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Configuration.LocalDataPath = @"C:\Projects\Github\bro\App_Data\Shapes";
            //Configuration.LocalDataPath = @"G:\Temp\Shapes";
        }
    }
}
