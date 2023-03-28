using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace tharsis6A
{
    /// <summary>
    /// UserControl1.xaml etkileşim mantığı
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();

            init3D();
        }

        private const string path = "Part1.stl";
        Model3D device;
        double previousRoll = 0, previousPitch = 0, previousYaw = 0;

        public void init3D()
        {
            try
            {
                ModelVisual3D device3D = new ModelVisual3D();
                device3D.Content = Display3d(path);
                viewPort3d.Children.Add(device3D);
                viewPort3d.RotateGesture = new MouseGesture(MouseAction.LeftClick);
                viewPort3d.ShowCoordinateSystem = true;
                viewPort3d.Children.Add(new DefaultLights());

                var matrix = device.Transform.Value;
                matrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), 90));
                device.Transform = new MatrixTransform3D(matrix);

                matrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), 0));
                device.Transform = new MatrixTransform3D(matrix);

                matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), 180));
                device.Transform = new MatrixTransform3D(matrix);

                PerspectiveCamera camera = new PerspectiveCamera(new Point3D(0, 0, 0), new Vector3D(-12, -16, -4), new Vector3D(0, 0, 1), 30);
                viewPort3d.Camera = camera;
                viewPort3d.FixedRotationPointEnabled = true;
                viewPort3d.FixedRotationPoint = new Point3D(5, 5, 5);


            }
            catch (Exception err)
            {
                //System.Windows.Forms.MessageBox.Show(err.Message);
            }
        }


        private Model3D Display3d(string model)
        {
            device = null;
            try
            {
                //Import 3D model file
                ModelImporter import = new ModelImporter();

                //Load the 3D model file
                device = import.Load(model);
            }
            catch (Exception e)
            {
                // Handle exception in case can not find the 3D model file
                MessageBox.Show("Exception Error : " + e.StackTrace);
            }
            return device;
        }

        public void Rotate(double Roll, double Pitch, double Yaw)
        {
            try
            {
                double rollAngle = Roll - previousRoll;
                double rollPitch = Pitch - previousPitch;
                double rollYaw = Yaw - previousYaw;

                var centerR = new Point3D(0, 5, 7.5);
                var centerP = new Point3D(-12, 0, 7.5);
                var centerY = new Point3D(-12, 5, 0);

                var matrix = device.Transform.Value;
                matrix.RotateAt(new Quaternion(new Vector3D(1, 0, 0), rollAngle), centerR);
                device.Transform = new MatrixTransform3D(matrix);

                matrix.RotateAt(new Quaternion(new Vector3D(0, 1, 0), rollPitch), centerP);
                device.Transform = new MatrixTransform3D(matrix);

                matrix.RotateAt(new Quaternion(new Vector3D(0, 0, 1), rollYaw), centerY);
                device.Transform = new MatrixTransform3D(matrix);

                /* matrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), rollAngle));
                 device.Transform = new MatrixTransform3D(matrix);

                 matrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), rollPitch));
                 device.Transform = new MatrixTransform3D(matrix);

                 matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), rollYaw));
                 device.Transform = new MatrixTransform3D(matrix);*/

                previousRoll = Roll;
                previousPitch = Pitch;
                previousYaw = Yaw;
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.Message);
            }
        }

        public void resetView()
        {
            try
            {

            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.Message);
            }
        }
    }
}
