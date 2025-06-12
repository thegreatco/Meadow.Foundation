using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.mikroBUS.Displays;
using System;
using System.Threading.Tasks;

namespace C8800Retro_ProjectLab_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        C8800Retro altair;
        IProjectLabHardware projectLab;

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing ...");

            projectLab = ProjectLab.Create();

            altair = new C8800Retro(projectLab.MikroBus1.I2cBus, projectLab.MikroBus1.Pins.INT);

            var button1B = altair.GetButton(C8800Retro.ButtonColumn._1, C8800Retro.ButtonRow.B);
            button1B.Clicked += Button1B_Clicked;

            graphics = new MicroGraphics(altair)
            {
                CurrentFont = new Font4x8(),
            };

            return base.Initialize();
        }

        private void Button1B_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Button 1B clicked");
        }

        public override async Task Run()
        {
            altair.EnableBlink(true, true);

            graphics.Clear();
            graphics.DrawText(0, 0, "MF", Color.White);
            graphics.Show();

            await Task.Delay(6000);

            altair.EnableBlink(false);
        }

        //<!=SNOP=>
    }
}