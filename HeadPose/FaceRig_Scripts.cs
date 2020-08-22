using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace HeadPoseSharp
{
    public class FaceRig_Scripts : IHeadPose
    {

        public Mat<Point3f> Model_points { get; } = new Mat<Point3f>()
        {
            // tip nose
            new Point3f(0.0f,0.0f,0.0f),
            // chain
            new Point3f(0.0f,-330.0f,-65.0f),
            // left corner of left eye
            new Point3f(-225.0f,170.0f,-135.0f),
            // right corner of right eye
            new Point3f(225.0f,170.0f,-135.0f),
            // left corner of mouth
            new Point3f(-150.0f,-150.0f,-125.0f),
            // right corner of mouth
            new Point3f(150.0f,-150.0f,-125.0f)
        };


        public Angles GetAnglesAndPoints(Mat<Point2d> points, int width, int height)
        {

            using (var objPtsMat = InputArray.Create(Model_points, MatType.CV_32FC3))//new Mat(objPts.Count, 1, MatType.CV_32FC3, objPts))
            using (var imgPtsMat = InputArray.Create(points, MatType.CV_32FC2))//new Mat(imgPts.Length, 1, MatType.CV_32FC2, imgPts))
            using (var cameraMatrixMat = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (var distMat = Mat.Zeros(5, 0, MatType.CV_64FC1))
            using (var rvecMat = new Mat())
            using (var tvecMat = new Mat())
            {
                Cv2.SolvePnP(objPtsMat, imgPtsMat, cameraMatrixMat, distMat, rvecMat, tvecMat);

                using (Mat resultPoints = new Mat())
                {
                    Cv2.ProjectPoints(objPtsMat, rvecMat, tvecMat, cameraMatrixMat, distMat, resultPoints);
                }

                // 根据旋转矩阵求解坐标旋转角
                double theta_x = Math.Atan2((float)rvecMat.At<double>(2, 1), (float)rvecMat.At<double>(2, 2));
                double theta_y = Math.Atan2((float)-rvecMat.At<double>(2, 0),
                     Math.Sqrt((float)((rvecMat.At<double>(2, 1) * rvecMat.At<double>(2, 1)) + ((float)rvecMat.At<double>(2, 2) * rvecMat.At<double>(2, 2)))));
                double theta_z = Math.Atan2((float)rvecMat.At<double>(1, 0), (float)rvecMat.At<double>(0, 0));

                // 将弧度转为角度
                Angles angles = new Angles();
                angles.Roll = theta_x * (180 / Math.PI);
                angles.Pitch = theta_y * (180 / Math.PI);
                angles.Yaw = theta_z * (180 / Math.PI);

                // 将映射的点的坐标保存下来
                // outarray类型的resultpoints如何转存到list中？
                return angles;
            }
        }

    }


}
