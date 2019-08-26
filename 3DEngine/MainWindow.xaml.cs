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
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.DataGrid;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using _3DEngine.ModelSpace;
using MathNet.Numerics.LinearAlgebra;
using SharpDX;
using SharpDX.Mathematics;
using SharpDX.Mathematics.Interop;

namespace _3DEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Texture texture;//tekstura dla modelu
        Model model;//wyswietlany model
        Scene scene;//obiekt na ktorym wyświetlamy
        Camera camera = new Camera(); // kamera
        Transformations transMatrices = new Transformations(); //macierze transformacji (obiekt odpowiadajacy za transformacje)

        float moveit = 0.0523F;//kat o jaki obracamy przy jednorazowym nacisnieciu strzalki
        float xAngle = 0;
        float yAngle = 0;

        //liczenie fps
        DateTime previousDate;
        int sampleCount;
        int framesNumber;
        public MainWindow()
        {
            InitializeComponent();

            scene = new Scene((int)image.Width, (int)image.Height, System.Windows.Media.Color.FromRgb(255, 255, 255));
            image.Source = scene.scene;

            camera.Position = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, 10.0f, 1 });
            camera.Target = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, 0,0 });

            sampleCount = 0;
            framesNumber = 0;

            CompositionTarget.Rendering += ModelRendering;

        }

        void ModelRendering(object sender, object e)
        {
            DateTime now = DateTime.Now;
            int fps;
            if (now != previousDate)
              fps = (int)(1000.0 / (now - previousDate).TotalMilliseconds);
            else fps = 1000;
            sampleCount++;
            framesNumber += fps;
            previousDate = now;

            if (sampleCount == 60)
            {
                sampleCount = 0;
                framesNumber /= 60;
                speed.Content = framesNumber.ToString();
                framesNumber = 0;
            }

            if (model!=null)
            {
                scene.clear();

                // odrysowywanie modelu
                model.draw(camera, model, scene, transMatrices.rotateX, transMatrices.rotateY, transMatrices.translate);
                // wrzucenie backbuffora do frontbuffora
                scene.swapBuffer();
            }

        }

        private void loadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Model files (*.off)|*.off";

            if (openFileDialog.ShowDialog() == true)
            {
                //wczytanie pliku .off
                model = LoadOffFile(openFileDialog.FileName);
                transMatrices = new Transformations();
                xAngle = 0;
                yAngle = 0;
                distanceSlider.Value = -10;
                gridONBt.IsChecked = true;
            }

        }

        //wczytanie tekstury
        private void loadTexture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage textureimage = new BitmapImage(new Uri(openFileDialog.FileName));
                textureImg.Source = textureimage;

                texture = new Texture(textureimage);
            }

        }


        private Model LoadOffFile(string filename)
        {
            CultureInfo Culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            Culture.NumberFormat.CurrencyDecimalSeparator = ".";

            StreamReader reader = new StreamReader(filename);

            int verticesCount = 0, facesCount = 0, edgesCount = 0;
            List<ModelSpace.Point3D> vertices = new List<ModelSpace.Point3D>();
            List<ModelSpace.Polygon> faces = new List<ModelSpace.Polygon>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().ToLower();
                if (line.Trim() == string.Empty || line.StartsWith("#"))
                {
                    continue;
                }

                string[] vals = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (line == "off")
                {
                    continue;
                }
                else
                {
                    verticesCount = int.Parse(vals[0]);
                    facesCount = int.Parse(vals[1]);
                    edgesCount = int.Parse(vals[2]);

                    break;
                }

            }

            
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().ToLower();
                if (line.Trim() == string.Empty || line.StartsWith("#"))
                {
                    continue;
                }

                var vals = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                float x = float.Parse(vals[0], NumberStyles.Any, Culture);
                float y = float.Parse(vals[1], NumberStyles.Any, Culture);
                float z = float.Parse(vals[2], NumberStyles.Any, Culture);

                ModelSpace.Point3D newPoint = new ModelSpace.Point3D();
                newPoint.dim = Vector<float>.Build.DenseOfArray(new float[] { x, y, z, 1 });

                vertices.Add(newPoint);

                --verticesCount;
                if (verticesCount == 0) break;

            }

            ModelSpace.Point3D[] vertex = vertices.ToArray();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().ToLower();
                if (line.Trim() == string.Empty || line.StartsWith("#"))
                {
                    continue;
                }

                var vals = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //wyliczenie wektora normalnego
                Vector<double> normal = CalculateNormal(vertex[int.Parse(vals[1])], vertex[int.Parse(vals[2])], vertex[int.Parse(vals[3])]);

                //stworzenie pojedyńczej ściany
                faces.Add(new ModelSpace.Polygon(int.Parse(vals[1]),int.Parse(vals[2]), int.Parse(vals[3]), Vector<float>.Build.DenseOfArray(new float[] { (int)normal[0], (int)normal[1], (int)normal[2],1 })));

            }

            return new Model(vertex, faces, (int)image.Width, (int)image.Height, scene);
        }

        private Vector<double> CalculateNormal(ModelSpace.Point3D a, ModelSpace.Point3D b, ModelSpace.Point3D c)
        {
            Vector<double> v1 = Vector<double>.Build.DenseOfArray(new double[] { a.dim[0], a.dim[1], a.dim[2] });
            Vector<double> v2 = Vector<double>.Build.DenseOfArray(new double[] { b.dim[0], b.dim[1], b.dim[2] });
            Vector<double> v3 = Vector<double>.Build.DenseOfArray(new double[] { c.dim[0], c.dim[1], c.dim[2] });
            Vector<double> A = v2 - v1;
            Vector<double> B = v3 - v1;
            Vector<double> normal = Vector<double>.Build.DenseOfArray(new double[] { A[1] * B[2] - A[2] * B[1], A[2] * B[0] - A[0] * B[2], A[0] * B[1] - A[2] * B[0] });
            return normal.Normalize(1);
        }

        private void gridONBt_Click(object sender, RoutedEventArgs e)
        {
            model.changeType(ModelType.GRID, null);
        }

        private void plainColorsBt_Click(object sender, RoutedEventArgs e)
        {
            model.changeType(ModelType.COLOR, null);
        }

        private void textureBt_Click(object sender, RoutedEventArgs e)
        {
            model.changeType(ModelType.TEXTURE, texture);
        }

        private void distanceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (transMatrices != null && model != null)
            {
                transMatrices.scale((float)distanceSlider.Value);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                xAngle += moveit;
                transMatrices.rotateXAxe(xAngle);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                xAngle -= moveit;
                transMatrices.rotateXAxe(xAngle);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                yAngle -= moveit;
                transMatrices.rotateYAxe(yAngle);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                yAngle += moveit;
                transMatrices.rotateYAxe(yAngle);
                e.Handled = true;
            }
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Focus();
        }

        private void cullingON_Checked(object sender, RoutedEventArgs e)
        {
            model.culling = true;
        }

        private void cullingON_Unchecked(object sender, RoutedEventArgs e)
        {
            model.culling = false;
        }
    }
}
