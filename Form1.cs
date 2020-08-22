using FaceRecognitionDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HeadPoseSharp
{
    public partial class Form1 : Form
    {
        // 当前的旋转角度
        public Angles angles = new Angles();

        private readonly FaceRecognition _faceRecognition;
        public Form1()
        {
            InitializeComponent();
            //https://github.com/ageitgey/face_recognition_models/tree/master/face_recognition_models/models
            _faceRecognition = FaceRecognition.Create(@"F:\Environmental\face_recognition_models");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog folder = new OpenFileDialog();
            folder.ShowDialog();

            //folder.Multiselect = true;
            for (int i = 0; i < folder.FileNames.Length; i++)
            {
                string fileName = folder.FileNames[i];
                using (var evf_Bmp = new Bitmap(fileName))
                using (var dliImg = FaceRecognition.LoadImage(evf_Bmp))
                {
                    IEnumerable<IDictionary<FacePart, IEnumerable<FacePoint>>> landmarks = _faceRecognition.FaceLandmark(dliImg);
                    Graphics g = Graphics.FromImage(evf_Bmp);
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;


                    foreach (IDictionary<FacePart, IEnumerable<FacePoint>> item in landmarks)
                    {
                        Color color = Color.GreenYellow;
                        Mat<Point2d> imgPoint = new Mat<Point2d>();
                        FacePoint[] noseTip = item[FacePart.NoseBridge].ToArray();
                        FacePoint[] chin = item[FacePart.Chin].ToArray();
                        FacePoint[] leftEye = item[FacePart.LeftEye].ToArray();
                        FacePoint[] rightEye = item[FacePart.RightEye].ToArray();
                        FacePoint[] bottomLip = item[FacePart.BottomLip].ToArray();

#if DEBUG
                        foreach (var point in noseTip)
                        {
                            g.FillEllipse(new SolidBrush(Color.Red), (float)point.Point.X - 1, (float)point.Point.Y - 1, 3, 3);
                        }
                        foreach (var point in chin)
                        {
                            g.FillEllipse(new SolidBrush(Color.Red), (float)point.Point.X - 1, (float)point.Point.Y - 1, 3, 3);
                        }
                        foreach (var point in leftEye)
                        {
                            g.FillEllipse(new SolidBrush(Color.Red), (float)point.Point.X - 1, (float)point.Point.Y - 1, 3, 3);
                        }
                        foreach (var point in rightEye)
                        {
                            g.FillEllipse(new SolidBrush(Color.Red), (float)point.Point.X - 1, (float)point.Point.Y - 1, 3, 3);
                        }
                        foreach (var point in bottomLip)
                        {
                            g.FillEllipse(new SolidBrush(Color.Red), (float)point.Point.X - 1, (float)point.Point.Y - 1, 3, 3);
                        }
#endif

                        imgPoint.Add(new Point2d(noseTip[3].Point.X, noseTip[3].Point.Y));         // Nose tip                 鼻尖
                        imgPoint.Add(new Point2d(chin[8].Point.X, chin[8].Point.Y));               // Chin                     下巴
                        imgPoint.Add(new Point2d(leftEye[0].Point.X, leftEye[0].Point.Y));         // Left eye left corner     左眼左上角
                        imgPoint.Add(new Point2d(rightEye[3].Point.X, rightEye[3].Point.Y));       // Right eye right corner   右眼右上角
                        imgPoint.Add(new Point2d(bottomLip[6].Point.X, bottomLip[6].Point.Y));     // Left Mouth corner        左嘴角
                        imgPoint.Add(new Point2d(bottomLip[0].Point.X, bottomLip[0].Point.Y));     // Right mouth corner       右嘴角



                        foreach (var point in imgPoint)
                        {
                            g.FillEllipse(new SolidBrush(color), (float)point.X - 1, (float)point.Y - 1, 3, 3);
                        }



                        var rotation = HeadPose(item, dliImg.Width, dliImg.Height);
                        g.DrawString(rotation.Item1.ToString("0.000"), new Font("微软雅黑", 20F, FontStyle.Bold), Brushes.White, 10, 10);
                        g.DrawString(rotation.Item2.ToString("0.000"), new Font("微软雅黑", 20F, FontStyle.Bold), Brushes.White, 10, 30);
                        g.DrawString(rotation.Item3.ToString("0.000"), new Font("微软雅黑", 20F, FontStyle.Bold), Brushes.White, 10, 50);
                        imgPoint.Dispose();
                        break;

                    }
                    g.Dispose();
                    evf_Bmp.Save($"{Path.GetDirectoryName(fileName)}\\{Path.GetFileNameWithoutExtension(fileName)}_point.{Path.GetExtension(fileName)}");
                }



            }

        }


        private static readonly Mat<Point3d> model_points = new Mat<Point3d>(1, 6,
                     new Point3d[] {
                    new Point3d(0.0f, 0.0f, 0.0f),
                    new Point3d(0.0f, -330.0f, -65.0f),
                    new Point3d(-225.0f, 170.0f, -135.0f),
                    new Point3d(225.0f, 170.0f, -135.0f),
                    new Point3d(-150.0f, -150.0f, -125.0f),
                    new Point3d(150.0f, -150.0f, -125.0f)
                     });
        public static (double, double, double) HeadPose(IDictionary<FacePart, IEnumerable<FacePoint>> item, int width, int height)
        {
            using (Mat<Point2d> landmarks = new Mat<Point2d>())
            {
                FacePoint[] noseTip = item[FacePart.NoseBridge].ToArray();
                FacePoint[] chin = item[FacePart.Chin].ToArray();
                FacePoint[] leftEye = item[FacePart.LeftEye].ToArray();
                FacePoint[] rightEye = item[FacePart.RightEye].ToArray();
                FacePoint[] bottomLip = item[FacePart.BottomLip].ToArray();

                landmarks.Add(new Point2d(noseTip[4].Point.X, noseTip[4].Point.Y));         // Nose tip                 鼻尖
                landmarks.Add(new Point2d(chin[8].Point.X, chin[8].Point.Y));               // Chin                     下巴
                landmarks.Add(new Point2d(leftEye[0].Point.X, leftEye[0].Point.Y));         // Left eye left corner     左眼左上角
                landmarks.Add(new Point2d(rightEye[3].Point.X, rightEye[3].Point.Y));       // Right eye right corner   右眼右上角
                landmarks.Add(new Point2d(bottomLip[6].Point.X, bottomLip[6].Point.Y));     // Left Mouth corner        左嘴角
                landmarks.Add(new Point2d(bottomLip[0].Point.X, bottomLip[0].Point.Y));     // Right mouth corner       右嘴角

                using (Mat camera_matrix = GetCameraMatrix(width, height))
                using (Mat<double> coeffs = new Mat<double>(4, 1))
                {
                    coeffs.SetTo(0);
                    using (Mat<double> rotation = new Mat<double>())
                    {
                        using (Mat translation = new Mat<double>())
                        {
                            Cv2.SolvePnP(model_points, landmarks, camera_matrix, coeffs, rotation, translation);
                        }

                        var ang = GetEulerAngle(rotation);

                        return ang;
                    }
                }
            }
        }

        /// <summary>
        /// 旋转向量转化为欧拉角
        /// </summary>
        /// <param name="rotation_vector"></param>
        /// <returns>(Y, X, Z)</returns>
        private static (double, double, double) GetEulerAngle(Mat<double> rotation_vector)
        {
            var rotation_list = rotation_vector.ToArray();
            Mat mat = new Mat(3, 1, MatType.CV_64FC1, rotation_list);
            var theta = Cv2.Norm(mat, NormTypes.L2);
            var w = Math.Cos(theta / 2);
            var x = Math.Sin(theta / 2) * rotation_list[0] / theta;
            var y = Math.Sin(theta / 2) * rotation_list[1] / theta;
            var z = Math.Sin(theta / 2) * rotation_list[2] / theta;

            var ysqr = y * y;

            // pitch (x-axis rotation)
            var t0 = 2.0 * (w * x + y * z);
            var t1 = 1.0 - 2.0 * (x * x + ysqr);
            var pitch = Math.Atan2(t0, t1);//反正切（给坐标轴，x，y）

            // yaw (y-axis rotation)
            var t2 = 2.0 * (w * y - z * x);
            if (t2 > 1.0) t2 = 1.0;
            if (t2 < -1.0) t2 = -1.0;
            var yaw = Math.Asin(t2); //反正弦函数

            // roll (z-axis rotation)
            var t3 = 2.0 * (w * z + x * y);
            var t4 = 1.0 - 2.0 * (ysqr + z * z);
            var roll = Math.Atan2(t3, t4);

            // 单位转换：将弧度转换为度
            var Y = (pitch / Math.PI) * 180;
            Y = Math.Sign(Y) * 180 - Y;

            var X = (yaw / Math.PI) * 180;
            var Z = (roll / Math.PI) * 180;

            return (Y, X, Z);
        }

        public static Mat GetCameraMatrix(int width, int height)
        {
            return new Mat<double>(3, 3,
                new double[] {
                    width, 0,     width / 2,
                    0,     width, height / 2,
                    0,     0,     1
                });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (Points == null)
                return;
            GetAng(new FaceDlib_Core());
        }
        private void GetAng(IHeadPose headPose)
        {
            var angles = headPose.GetAnglesAndPoints(Points, width, height);
            string item = $"{headPose.GetType().Name}:\t{angles.Pitch.ToString("0.000000")} {angles.Roll.ToString("0.000000")} {angles.Yaw.ToString("0.000000")}";
            listBox1.Items.Add(item);
            Clipboard.SetDataObject(item, true);

        }
        private void button2_Click(object sender, EventArgs e)
        {
            //有异常
            if (Points == null)
                return;
            GetAng(new FaceRig_Scripts());

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Points == null)
                return;
            GetAng(new Emgu_Dlib_OpenCv());

        }
        public Mat<Point2d> Points { get; set; }
        int width;
        int height;
        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var opRes = openFileDialog.ShowDialog();
            if (opRes != DialogResult.OK)
                return;
            openFileDialog.Dispose();
            textBox1.Text = openFileDialog.FileName;
            using (var dliImg = FaceRecognition.LoadImageFile(textBox1.Text))
            {
                IEnumerable<IDictionary<FacePart, IEnumerable<FacePoint>>> landmarks = _faceRecognition.FaceLandmark(dliImg);
                foreach (IDictionary<FacePart, IEnumerable<FacePoint>> item in landmarks)
                {
                    Mat<Point2d> points = new Mat<Point2d>();
                    FacePoint[] noseTip = item[FacePart.NoseBridge].ToArray();
                    FacePoint[] chin = item[FacePart.Chin].ToArray();
                    FacePoint[] leftEye = item[FacePart.LeftEye].ToArray();
                    FacePoint[] rightEye = item[FacePart.RightEye].ToArray();
                    FacePoint[] bottomLip = item[FacePart.BottomLip].ToArray();

                    points.Add(new Point2d(noseTip[4].Point.X, noseTip[4].Point.Y));         // Nose tip                 鼻尖
                    points.Add(new Point2d(chin[8].Point.X, chin[8].Point.Y));               // Chin                     下巴
                    points.Add(new Point2d(leftEye[0].Point.X, leftEye[0].Point.Y));         // Left eye left corner     左眼左上角
                    points.Add(new Point2d(rightEye[3].Point.X, rightEye[3].Point.Y));       // Right eye right corner   右眼右上角
                    points.Add(new Point2d(bottomLip[6].Point.X, bottomLip[6].Point.Y));     // Left Mouth corner        左嘴角
                    points.Add(new Point2d(bottomLip[0].Point.X, bottomLip[0].Point.Y));     // Right mouth corner       右嘴角
                    Points = points;
                    width = dliImg.Width;
                    height = dliImg.Height;
                    break;

                }
            }
        }

    }

}
