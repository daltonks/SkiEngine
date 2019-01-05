using System;
using System.Diagnostics;

namespace SkiEngine.TestApp.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            try
            {
                this.InitializeComponent();

                LoadApplication(new TestApp.App());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw ex;
            }
        }
    }
}
