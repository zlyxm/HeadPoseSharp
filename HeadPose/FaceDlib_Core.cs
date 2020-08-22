using OpenCvSharp;
using System;

namespace HeadPoseSharp
{
    public class FaceDlib_Core : IHeadPose
    {
        public Mat<Point3f> Model_points => new Mat<Point3f>
             {
                        new Point3f(0.0f, 0.0f, 0.0f),             // Nose tip
                        new Point3f(0.0f, -330.0f, -65.0f),        // Chin
                        new Point3f(-225.0f, 170.0f, -135.0f),     // Left eye left corner
                        new Point3f(225.0f, 170.0f, -135.0f),      // Right eye right corne
                        new Point3f(-150.0f, -150.0f, -125.0f),    // Left Mouth corner
                        new Point3f(150.0f, -150.0f, -125.0f)      // Right mouth corner
             };
        public Angles GetAnglesAndPoints(Mat<Point2d> points, int width, int height)
        {
            var cameraMatrix = new Mat<double>(3, 3,
               new double[] {
                    width, 0,     width / 2,
                    0,     width, height / 2,
                    0,     0,     1
               });

            var dist = new Mat<double> { 0, 0, 0, 0, 0 };
            var rvec = new Mat<double>();
            var tvec = new Mat();
            var Model_points = new Mat<Point3f>
             {
                        new Point3f(0.0f, 0.0f, 0.0f),             // Nose tip
                        new Point3f(0.0f, -330.0f, -65.0f),        // Chin
                        new Point3f(-225.0f, 170.0f, -135.0f),     // Left eye left corner
                        new Point3f(225.0f, 170.0f, -135.0f),      // Right eye right corne
                        new Point3f(-150.0f, -150.0f, -125.0f),    // Left Mouth corner
                        new Point3f(150.0f, -150.0f, -125.0f)      // Right mouth corner
             };
            Cv2.SolvePnP(Model_points, points, cameraMatrix, dist, rvec, tvec, flags: SolvePnPFlags.Iterative);

            return GetEulerAngle(rvec);
        }


        /// 旋转向量转化为欧拉角
        public Angles GetEulerAngle(Mat<double> rotation_vector)
        {
            //var X = rotation_vector[0];
            //var Y = rotation_vector[1];
            //var Z = rotation_vector[2];

            //var x = Math.Sin(Y / 2)*Math.Sin(Z / 2)*Math.Cos(X / 2) + Math.Cos(Y / 2)*Math.Cos(Z / 2)*Math.Sin(X / 2);
            //var y = Math.Sin(Y / 2)*Math.Cos(Z / 2)*Math.Cos(X / 2) + Math.Cos(Y / 2)*Math.Sin(Z / 2)*Math.Sin(X / 2);
            //var z = Math.Cos(Y / 2)*Math.Sin(Z / 2)*Math.Cos(X / 2) - Math.Sin(Y / 2)*Math.Cos(Z / 2)*Math.Sin(X / 2);
            //var w = Math.Cos(Y / 2)*Math.Cos(Z / 2)*Math.Cos(X / 2) - Math.Sin(Y / 2)*Math.Sin(Z / 2)*Math.Sin(X / 2);

            var rotArray = rotation_vector.ToArray();

            Mat mat = new Mat(3, 1, MatType.CV_64FC1, rotArray);
            var theta = Cv2.Norm(mat, NormTypes.L2);
            var w = Math.Cos(theta / 2);
            var x = Math.Sin(theta / 2) * rotArray[0] / theta;
            var y = Math.Sin(theta / 2) * rotArray[1] / theta;
            var z = Math.Sin(theta / 2) * rotArray[2] / theta;

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
            Angles angles = new Angles() { Pitch = Y, Roll = Z, Yaw = X };
            return angles;
        }

    }


}
