using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using SharpDX.Mathematics;
using SharpDX;
using SharpDX.Mathematics.Interop;

namespace _3DEngine.ModelSpace
{
    public enum ModelType
    {
        GRID,
        COLOR,
        TEXTURE
    }
    public class Model
    {
        public Point3D[] Vertices; //wierzcholki modelu

        public Polygon[] Faces;   //sciany modelu

        ModelType type; //jak ma byc wyswietlany model

        Scene scene; // scena na ktorej malowany jest model

        Texture texture;

        public bool culling;


        public Model(Point3D[] vertices, List<Polygon> faces, int width, int height, Scene scene)
        {
            Vertices = new Point3D[vertices.Count()]; 
            vertices.CopyTo(Vertices,0);
            Faces = faces.ToArray();
            type = ModelType.GRID;
            this.scene = scene;
            culling = false;
        }

        //wyliczenie nowych wartosci punktu po przeksztalceniach oraz pozyskanie wszpolrzednych punktu na plaszczyznie
        private Point3D CalculateCordinates(Point3D vertex, Matrix<float> transformationM)
        {
            //pomnozenie prze macierz transformacji
            Vector<float> vec = vertex.dim * transformationM;

            //normalizacja
            vec[0] /= vec[3];
            vec[1] /= vec[3];
            vec[2] /= vec[3];
            vec[3] /= vec[3];

            //przesuniecie wspolrzednych, poniewaz rysujemy od lewego gornego rogu
            vec[0] = vec[0] * scene.width / 2;
            vec[1] = vec[1] * scene.height / 2;

            vec[0] += scene.width / 2;
            vec[1] = scene.height - (vec[1] + scene.height / 2) - 1;

            return new Point3D((float)vec[0], (float)vec[1], (float)vec[2], vertex.texturePoint);
        }

        public void draw(Camera camera, Model model, Scene scene, Matrix<float> rotateXM, Matrix<float> rotateYM, Matrix<float> translateM)
        {
            Vector<float> cameraRot, cameraToPolygon; //kamera po obrocie

            Matrix<float> rotX = Matrix<float>.Build.DenseOfMatrix(rotateXM);
            rotX[2, 1] = -rotX[2, 1];
            rotX[1, 2] = -rotX[1, 2];
            Matrix<float> rotY = Matrix<float>.Build.DenseOfMatrix(rotateYM);
            rotY[0, 2] = -rotY[0, 2];
            rotY[2, 0] = -rotY[2, 0];

            cameraRot = camera.Position * (rotY) * (rotX);

            
            var transformationM = rotateXM * rotateYM * translateM * scene.projectionMatrix;

            Parallel.For(0, Faces.Length, faceIndex =>
            {
                var face = Faces[faceIndex];
                //wierzcholki trojkata
                Point3D vertex1 = model.Vertices[face.indVertex1];
                Point3D vertex2 = model.Vertices[face.indVertex2];
                Point3D vertex3 = model.Vertices[face.indVertex3];

                //backface culling
                cameraToPolygon = cameraRot - vertex1.dim;


                if (!culling || (culling && (cameraToPolygon.DotProduct(face.normal) <= 0)))
                {
                    Point3D pixel1 = CalculateCordinates(vertex1, transformationM);
                    Point3D pixel2 = CalculateCordinates(vertex2, transformationM);
                    Point3D pixel3 = CalculateCordinates(vertex3, transformationM);

                    if (type == ModelType.GRID)
                    {
                        DrawLine(pixel1.dim, pixel2.dim, face.color);
                        DrawLine(pixel2.dim, pixel3.dim, face.color);
                        DrawLine(pixel3.dim, pixel1.dim, face.color);
                    }
                    else
                    {
                        DrawTriangle(pixel1, pixel2, pixel3, face.color, texture);
                    }
                    faceIndex++;
                }
            });

        }

        void DrawLine(Vector<float> point0, Vector<float> point1, System.Windows.Media.Color color)
        {
            int dx = (int)Math.Abs(point1[0] - point0[0]); //roznica odleglosci w poziomie
            int dy = (int)Math.Abs(point1[1] - point0[1]); //roznica odleglosci w pionie
            int d;
            int incrBOK, incrSKOS;
            int xi, yi; //zmniejszamy badz zwiekszamy w zaleznosci w ktora strone rysujemy
            int x = (int)point0[0], y = (int)point0[1], z = (int)point0[2];
            float gradientz;


            if (point0[0] < point1[0])
                xi = 1;     //rysujemy w prawo
            else
                xi = -1;    //rysujemy w lewo

            if (point0[1] < point1[1])
                yi = 1;     //rysujemy w dol
            else
                yi = -1;   //rysujemy w gore


            scene.setPixel(x, y, z,color);

            // oś wiodąca OX
            if (dx > dy)
            {
                incrSKOS = 2 * (dy - dx);
                incrBOK = 2 * dy;
                d = 2 * dy - dx;

                while (x != (int)point1[0])
                {
                    // test współczynnika
                    if (d < 0)
                    {
                        d += incrBOK;
                        x += xi;
                    }
                    else
                    {
                        d += incrSKOS;
                        x += xi;
                        y += yi;

                    }

                    gradientz = (point0[0] != point1[0]) ? (x - point0[0]) / (point1[0] - point0[0]):1;
                    z =(int) interpolate(point0[2], point1[2], gradientz);
                    scene.setPixel(x, y, z, color);
                }
            }
            // oś wiodąca OY
            else
            {
                incrSKOS = 2 * (dx - dy);
                incrBOK = 2 * dx;
                d = 2 * dx - dy;

                while (y != (int)point1[1])
                {
                    // test współczynnika
                    if (d < 0)
                    {
                        d += incrBOK;
                        y += yi;
                    }
                    else
                    {
                        d += incrSKOS;
                        x += xi;
                        y += yi;
                    }

                    gradientz = (point0[1] != point1[1]) ? (x - point0[1]) / (point1[1] - point0[1]) : 1;
                    z = (int)interpolate(point0[2], point1[2], gradientz);
                    scene.setPixel(x, y, z,color);
                }
            }
        }

        float interpolate(double min, double max, float gradient)
        {
            return (float)(min + (max - min) * Math.Max(0, Math.Min(gradient, 1)));
        }

        void ProcessScanLine(int y, Point3D pa, Point3D pb, Point3D pc, Point3D pd, System.Windows.Media.Color color , Texture texture)
        {
            //gradienty do interpolacji dla obu boków
            float gradient1 = pa.dim[1] != pb.dim[1] ? (y - pa.dim[1]) / (pb.dim[1] - pa.dim[1]) : 1;
            float gradient2 = pc.dim[1] != pd.dim[1] ? (y - pc.dim[1]) / (pd.dim[1] - pc.dim[1]) : 1;

            //poczatek i koniec scanlinii
            int x1 = (int)interpolate(pa.dim[0], pb.dim[0], gradient1);
            int x2 = (int)interpolate(pc.dim[0], pd.dim[0], gradient2);

            //wspolrzedne z dla x1 i x2
            float z1 = interpolate(pa.dim[2], pb.dim[2], gradient1);
            float z2 = interpolate(pc.dim[2], pd.dim[2], gradient2);

            //interpolowane wspolrzedne tekstury
            var u1 = interpolate(pa.texturePoint[0], pb.texturePoint[0], gradient1);
            var u2 = interpolate(pc.texturePoint[0], pd.texturePoint[0], gradient2);
            var s1 = interpolate(pa.texturePoint[1], pb.texturePoint[1], gradient1);
            var s2 = interpolate(pc.texturePoint[1], pd.texturePoint[1], gradient2);

            if (x2 < x1)
            {
                int tmp = x1;
                x1 = x2;
                x2 = tmp;
            }
            
            //rysowanie scanlinii
            for (int x = x1; x < x2; x++)
            {
                float gradient = (x - x1) / (float)(x2 - x1);
                float z = interpolate(z1, z2, gradient);
                float u = interpolate(u1, u2, gradient);
                float v = interpolate(s1, s2, gradient);

                System.Windows.Media.Color pixelColor;

                if (texture != null && type == ModelType.TEXTURE)
                    pixelColor = texture.Map(u, v);
                else
                    pixelColor = color;

                scene.setPixel(x, y, z, pixelColor);
            }
        }

        void DrawTriangle(Point3D p1, Point3D p2, Point3D p3, System.Windows.Media.Color color, Texture texture)
        {
            //ustawiamy trojkat p1 na gorze potem p2 a potem p3
            Point3D tmp;
            if (p1.dim[1] > p2.dim[1])
            {
                tmp = p2;
                p2 = p1;
                p1 = tmp;
            }
            if (p2.dim[1] > p3.dim[1])
            {
                tmp = p2;
                p2 = p3;
                p3 = tmp;
            }
            if (p1.dim[1] > p2.dim[1])
            {
                tmp = p2;
                p2 = p1;
                p1 = tmp;
            }

            //odwrotnosc nachylenia bokow p1p2 i p1p3
            float m1 = (p2.dim[1] - p1.dim[1] > 0) ? (p2.dim[0] - p1.dim[0]) / (p2.dim[1] - p1.dim[1]) : 0;
            float m2 = (p3.dim[1] - p1.dim[1] > 0) ? (p3.dim[0] - p1.dim[0]) / (p3.dim[1] - p1.dim[1]) : 0;

            for (var y = (int)p1.dim[1]; y <= (int)p3.dim[1]; y++)
            {
                //dla kazdej scanlinii
                //p2 zprawej strony
                if (m1 > m2)
                {

                        if (y < p2.dim[1])
                        {
                            ProcessScanLine(y, p1, p3, p1, p2, color, texture);
                        }
                        else
                        {
                            ProcessScanLine(y, p1, p3, p2, p3, color, texture);
                        }
                    
                }
                //p2 z lewej strony
                else
                {        
                        if (y < p2.dim[1])
                        {
                            ProcessScanLine(y, p1, p2, p1, p3, color, texture);
                        }
                        else
                        {
                            ProcessScanLine(y, p2, p3, p1, p3, color, texture);
                        }               
                }
            }
              

        }


        //zmiana wygladu modelu
        public void changeType(ModelType newtype, Texture texture)
        {
            type = newtype;
            Random rnd = new Random(DateTime.Now.Millisecond);

            switch (type)
            {
                case ModelType.GRID:
                    {
                        this.texture = null;
                        foreach (var tri in Faces)
                        {
                            tri.colorChange(System.Windows.Media.Color.FromRgb(255, 255, 255));             
                        }
                        break;
                    }
                case ModelType.COLOR:
                    {
                        this.texture = null;
                        foreach (var tri in Faces)
                        {
                            System.Windows.Media.Color newcolor = new System.Windows.Media.Color();
                            newcolor.R = (byte)rnd.Next(0, 255);
                            newcolor.G = (byte)rnd.Next(0, 255);
                            newcolor.B = (byte)rnd.Next(0, 255);
                            tri.colorChange(newcolor);
                        }
                        break;
                    }
                case ModelType.TEXTURE:
                    {
                        if (texture != null)
                        {
                            this.texture = texture;
                            foreach (var tri in Faces)
                            {
                                //losowanie punktow tekstury
                                this.Vertices[tri.indVertex1].texturePoint = Vector<double>.Build.DenseOfArray(new double[] { rnd.Next(0, texture.width), rnd.Next(0, texture.height) });
                                this.Vertices[tri.indVertex2].texturePoint = Vector<double>.Build.DenseOfArray(new double[] { rnd.Next(0, texture.width), rnd.Next(0, texture.height) });
                                this.Vertices[tri.indVertex3].texturePoint = Vector<double>.Build.DenseOfArray(new double[] { rnd.Next(0, texture.width), rnd.Next(0, texture.height) });
                                tri.colorChange(System.Windows.Media.Color.FromRgb(0, 0, 0));
                            }
                        }

                        break;
                    }
            }
        }

    }
}
